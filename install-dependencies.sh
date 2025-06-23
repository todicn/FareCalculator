#!/bin/bash

# Installation script for DocFX documentation monitoring dependencies

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}📦 Installing DocFX Documentation Monitor Dependencies${NC}"
echo ""

# Function to check if running as root
check_root() {
    if [[ $EUID -eq 0 ]]; then
        echo -e "${YELLOW}⚠️  Running as root. Some installations may not work correctly.${NC}"
        echo -e "${YELLOW}Consider running as a regular user with sudo when needed.${NC}"
        echo ""
    fi
}

# Function to detect OS
detect_os() {
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        if command -v apt-get &> /dev/null; then
            echo "ubuntu"
        elif command -v yum &> /dev/null; then
            echo "centos"
        elif command -v dnf &> /dev/null; then
            echo "fedora"
        else
            echo "linux"
        fi
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        echo "macos"
    else
        echo "unknown"
    fi
}

# Function to install inotify-tools
install_inotify_tools() {
    local os=$(detect_os)
    
    echo -e "${YELLOW}📦 Installing inotify-tools...${NC}"
    
    case $os in
        ubuntu)
            sudo apt-get update
            sudo apt-get install -y inotify-tools
            ;;
        centos)
            sudo yum install -y inotify-tools
            ;;
        fedora)
            sudo dnf install -y inotify-tools
            ;;
        macos)
            if command -v brew &> /dev/null; then
                brew install fswatch
            else
                echo -e "${RED}❌ Homebrew not found. Please install Homebrew first.${NC}"
                return 1
            fi
            ;;
        *)
            echo -e "${RED}❌ Unsupported OS for automatic installation${NC}"
            echo -e "${YELLOW}Please install inotify-tools manually${NC}"
            return 1
            ;;
    esac
}

# Function to install .NET
install_dotnet() {
    echo -e "${YELLOW}📦 Installing .NET SDK...${NC}"
    
    if command -v dotnet &> /dev/null; then
        echo -e "${GREEN}✅ .NET SDK already installed${NC}"
        dotnet --version
        return 0
    fi
    
    # Download and install .NET
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 8.0
    rm dotnet-install.sh
    
    # Add to PATH if not already there
    if ! echo "$PATH" | grep -q "$HOME/.dotnet"; then
        echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
        export PATH=$PATH:$HOME/.dotnet
    fi
    
    echo -e "${GREEN}✅ .NET SDK installed${NC}"
}

# Function to install DocFX
install_docfx() {
    echo -e "${YELLOW}📦 Installing DocFX...${NC}"
    
    if command -v docfx &> /dev/null; then
        echo -e "${GREEN}✅ DocFX already installed${NC}"
        docfx --version
        return 0
    fi
    
    # Install DocFX using dotnet tool
    if command -v dotnet &> /dev/null; then
        dotnet tool install -g docfx
        
        # Add dotnet tools to PATH if not already there
        if ! echo "$PATH" | grep -q "$HOME/.dotnet/tools"; then
            echo 'export PATH=$PATH:$HOME/.dotnet/tools' >> ~/.bashrc
            export PATH=$PATH:$HOME/.dotnet/tools
        fi
        
        echo -e "${GREEN}✅ DocFX installed${NC}"
    else
        echo -e "${RED}❌ .NET SDK not found. Please install .NET first.${NC}"
        return 1
    fi
}

# Function to verify installations
verify_installations() {
    echo -e "${BLUE}🔍 Verifying installations...${NC}"
    local all_ok=true
    
    # Check inotify-tools
    if command -v inotifywait &> /dev/null; then
        echo -e "${GREEN}✅ inotify-tools: $(inotifywait --version 2>&1 | head -n1)${NC}"
    else
        echo -e "${RED}❌ inotify-tools not found${NC}"
        all_ok=false
    fi
    
    # Check .NET
    if command -v dotnet &> /dev/null; then
        echo -e "${GREEN}✅ .NET SDK: $(dotnet --version)${NC}"
    else
        echo -e "${RED}❌ .NET SDK not found${NC}"
        all_ok=false
    fi
    
    # Check DocFX
    if command -v docfx &> /dev/null; then
        echo -e "${GREEN}✅ DocFX: $(docfx --version 2>&1 | grep -o '[0-9]\+\.[0-9]\+\.[0-9]\+' | head -n1)${NC}"
    else
        echo -e "${RED}❌ DocFX not found${NC}"
        all_ok=false
    fi
    
    if $all_ok; then
        echo -e "${GREEN}🎉 All dependencies installed successfully!${NC}"
        return 0
    else
        echo -e "${RED}❌ Some dependencies are missing${NC}"
        return 1
    fi
}

# Main installation process
main() {
    check_root
    
    local os=$(detect_os)
    echo -e "${BLUE}🖥️  Detected OS: $os${NC}"
    echo ""
    
    # Install dependencies
    if ! command -v inotifywait &> /dev/null; then
        install_inotify_tools || exit 1
    else
        echo -e "${GREEN}✅ inotify-tools already installed${NC}"
    fi
    
    if ! command -v dotnet &> /dev/null; then
        install_dotnet || exit 1
    else
        echo -e "${GREEN}✅ .NET SDK already installed${NC}"
    fi
    
    if ! command -v docfx &> /dev/null; then
        install_docfx || exit 1
    else
        echo -e "${GREEN}✅ DocFX already installed${NC}"
    fi
    
    echo ""
    verify_installations
    
    echo ""
    echo -e "${BLUE}📝 Next steps:${NC}"
    echo -e "${YELLOW}1. Restart your terminal or run: source ~/.bashrc${NC}"
    echo -e "${YELLOW}2. Test the monitor: ./monitor-docs.sh --check${NC}"
    echo -e "${YELLOW}3. Start monitoring: ./monitor-docs.sh${NC}"
    echo -e "${YELLOW}4. Or run as daemon: ./monitor-docs.sh --daemon${NC}"
}

# Run the main function
main "$@"