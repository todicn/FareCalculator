#!/bin/bash

# DocFX Documentation Monitor Script
# Monitors changes in source files and automatically rebuilds documentation

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
DEBOUNCE_TIME=2  # Wait 2 seconds after last change before rebuilding
BUILD_SCRIPT="./build-docs.sh"
LOG_FILE="docs-monitor.log"

# Directories and files to monitor
WATCH_DIRS=(
    "src/"
    "docs/"
    "tests/"
    "api/"
)

WATCH_FILES=(
    "docfx.json"
    "README.md"
    "index.md"
    "toc.yml"
    "*.md"
)

# Function to log messages
log_message() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $1" | tee -a "$LOG_FILE"
}

# Function to build documentation
build_docs() {
    echo -e "${YELLOW}🔨 Building documentation...${NC}"
    log_message "Starting documentation build"
    
    if [ -x "$BUILD_SCRIPT" ]; then
        if $BUILD_SCRIPT >> "$LOG_FILE" 2>&1; then
            echo -e "${GREEN}✅ Documentation build completed successfully${NC}"
            log_message "Documentation build completed successfully"
            
            # Optional: Check if docfx serve is running and reload
            if pgrep -f "docfx serve" > /dev/null; then
                echo -e "${CYAN}🔄 DocFx serve detected - documentation will auto-reload${NC}"
                log_message "DocFx serve detected - documentation will auto-reload"
            fi
        else
            echo -e "${RED}❌ Documentation build failed - check $LOG_FILE for details${NC}"
            log_message "Documentation build failed"
        fi
    else
        echo -e "${RED}❌ Build script not found or not executable: $BUILD_SCRIPT${NC}"
        log_message "Build script not found or not executable: $BUILD_SCRIPT"
    fi
}

# Function to check if required tools are installed
check_dependencies() {
    local missing_deps=()
    
    if ! command -v inotifywait &> /dev/null; then
        missing_deps+=("inotify-tools")
    fi
    
    if ! command -v docfx &> /dev/null; then
        missing_deps+=("docfx")
    fi
    
    if ! command -v dotnet &> /dev/null; then
        missing_deps+=("dotnet")
    fi
    
    if [ ${#missing_deps[@]} -gt 0 ]; then
        echo -e "${RED}❌ Missing dependencies: ${missing_deps[*]}${NC}"
        echo -e "${YELLOW}Please install the missing dependencies:${NC}"
        echo -e "${YELLOW}  - For inotify-tools: sudo apt-get install inotify-tools${NC}"
        echo -e "${YELLOW}  - For docfx: Install from https://github.com/dotnet/docfx${NC}"
        echo -e "${YELLOW}  - For dotnet: Install from https://dotnet.microsoft.com/download${NC}"
        exit 1
    fi
}

# Function to setup monitoring
setup_monitoring() {
    local watch_paths=""
    
    # Add directories to watch
    for dir in "${WATCH_DIRS[@]}"; do
        if [ -d "$dir" ]; then
            watch_paths="$watch_paths $dir"
        fi
    done
    
    # Add individual files to watch
    for file in "${WATCH_FILES[@]}"; do
        if [[ "$file" == *"*"* ]]; then
            # Handle wildcard patterns
            watch_paths="$watch_paths ."
        elif [ -f "$file" ]; then
            watch_paths="$watch_paths $file"
        fi
    done
    
    echo "$watch_paths"
}

# Function to start monitoring
start_monitoring() {
    local watch_paths=$(setup_monitoring)
    local last_build_time=0
    
    echo -e "${GREEN}🚀 Starting DocFX documentation monitor...${NC}"
    echo -e "${CYAN}📁 Monitoring paths: $watch_paths${NC}"
    echo -e "${CYAN}📋 Log file: $LOG_FILE${NC}"
    echo -e "${CYAN}⏱️  Debounce time: ${DEBOUNCE_TIME}s${NC}"
    echo -e "${YELLOW}Press Ctrl+C to stop monitoring${NC}"
    echo ""
    
    log_message "Documentation monitor started"
    log_message "Monitoring paths: $watch_paths"
    
    # Initial build
    build_docs
    
    # Start monitoring
    inotifywait -m -r -e modify,create,delete,move \
        --exclude '\.(git|vs|vscode)/.*|.*\.(tmp|temp|swp|swo|log)$|bin/.*|obj/.*|_site/.*|TestResults/.*' \
        $watch_paths 2>/dev/null |
    while read -r directory events filename; do
        # Skip if it's a directory event without a filename
        if [ -z "$filename" ]; then
            continue
        fi
        
        # Check if the file is relevant
        case "$filename" in
            *.cs|*.csproj|*.md|*.yml|*.yaml|*.json)
                current_time=$(date +%s)
                
                # Debounce: only build if enough time has passed since last change
                if [ $((current_time - last_build_time)) -gt $DEBOUNCE_TIME ]; then
                    echo -e "${BLUE}📝 Detected change: $directory$filename${NC}"
                    log_message "File changed: $directory$filename"
                    
                    # Wait a bit more to catch rapid successive changes
                    sleep 1
                    
                    build_docs
                    last_build_time=$current_time
                else
                    echo -e "${YELLOW}⏳ Change detected but debouncing...${NC}"
                fi
                ;;
        esac
    done
}

# Function to show help
show_help() {
    echo "DocFX Documentation Monitor"
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "OPTIONS:"
    echo "  -h, --help     Show this help message"
    echo "  -d, --daemon   Run as daemon (background process)"
    echo "  -s, --stop     Stop any running monitor daemon"
    echo "  -c, --check    Check dependencies and configuration"
    echo "  -b, --build    Build documentation once and exit"
    echo ""
    echo "Examples:"
    echo "  $0              # Start monitoring (foreground)"
    echo "  $0 --daemon     # Start monitoring as daemon"
    echo "  $0 --build      # Build documentation once"
    echo "  $0 --stop       # Stop daemon"
}

# Function to run as daemon
run_as_daemon() {
    local pid_file="docs-monitor.pid"
    
    if [ -f "$pid_file" ]; then
        local existing_pid=$(cat "$pid_file")
        if ps -p "$existing_pid" > /dev/null 2>&1; then
            echo -e "${YELLOW}⚠️  Monitor daemon is already running (PID: $existing_pid)${NC}"
            echo -e "${YELLOW}Use '$0 --stop' to stop it first${NC}"
            exit 1
        else
            rm -f "$pid_file"
        fi
    fi
    
    echo -e "${GREEN}🚀 Starting documentation monitor as daemon...${NC}"
    nohup "$0" > /dev/null 2>&1 &
    local daemon_pid=$!
    echo "$daemon_pid" > "$pid_file"
    
    echo -e "${GREEN}✅ Daemon started with PID: $daemon_pid${NC}"
    echo -e "${CYAN}📋 Log file: $LOG_FILE${NC}"
    echo -e "${CYAN}🛑 To stop: $0 --stop${NC}"
}

# Function to stop daemon
stop_daemon() {
    local pid_file="docs-monitor.pid"
    
    if [ -f "$pid_file" ]; then
        local daemon_pid=$(cat "$pid_file")
        if ps -p "$daemon_pid" > /dev/null 2>&1; then
            kill "$daemon_pid"
            rm -f "$pid_file"
            echo -e "${GREEN}✅ Monitor daemon stopped (PID: $daemon_pid)${NC}"
        else
            echo -e "${YELLOW}⚠️  No running daemon found${NC}"
            rm -f "$pid_file"
        fi
    else
        echo -e "${YELLOW}⚠️  No daemon PID file found${NC}"
    fi
}

# Main execution
case "${1:-}" in
    -h|--help)
        show_help
        exit 0
        ;;
    -d|--daemon)
        check_dependencies
        run_as_daemon
        exit 0
        ;;
    -s|--stop)
        stop_daemon
        exit 0
        ;;
    -c|--check)
        echo -e "${CYAN}🔍 Checking dependencies and configuration...${NC}"
        check_dependencies
        echo -e "${GREEN}✅ All dependencies are installed${NC}"
        
        if [ -f "$BUILD_SCRIPT" ] && [ -x "$BUILD_SCRIPT" ]; then
            echo -e "${GREEN}✅ Build script found and executable${NC}"
        else
            echo -e "${YELLOW}⚠️  Build script not found or not executable${NC}"
        fi
        
        if [ -f "docfx.json" ]; then
            echo -e "${GREEN}✅ DocFx configuration found${NC}"
        else
            echo -e "${YELLOW}⚠️  DocFx configuration not found${NC}"
        fi
        exit 0
        ;;
    -b|--build)
        check_dependencies
        build_docs
        exit 0
        ;;
    "")
        check_dependencies
        start_monitoring
        ;;
    *)
        echo -e "${RED}❌ Unknown option: $1${NC}"
        show_help
        exit 1
        ;;
esac