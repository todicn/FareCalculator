# DocFX Documentation Monitoring System

This system automatically monitors changes in your codebase and documentation files, then rebuilds your DocFX documentation whenever changes are detected.

## Features

- 🔄 **Automatic Monitoring**: Watches source code, documentation files, and configuration changes
- ⚡ **Smart Debouncing**: Prevents excessive rebuilds during rapid file changes
- 🎛️ **Multiple Run Modes**: Foreground, daemon, or systemd service
- 📊 **Comprehensive Logging**: Detailed logs of all monitoring and build activities
- 🚀 **Hot Reload Support**: Automatically refreshes served documentation
- 🔍 **Dependency Checking**: Validates all required tools are installed
- 🎨 **Colorized Output**: Easy-to-read status messages and progress indicators

## Quick Start

### 1. Install Dependencies

```bash
./install-dependencies.sh
```

This will install:
- `inotify-tools` (for file system monitoring)
- `.NET SDK` (for building C# projects)
- `DocFX` (for generating documentation)

### 2. Test the Setup

```bash
./monitor-docs.sh --check
```

### 3. Build Documentation Once

```bash
./monitor-docs.sh --build
```

### 4. Start Monitoring

```bash
# Run in foreground (see live updates)
./monitor-docs.sh

# Run as background daemon
./monitor-docs.sh --daemon

# Stop daemon
./monitor-docs.sh --stop
```

## Usage Options

### Command Line Options

```bash
./monitor-docs.sh [OPTIONS]

OPTIONS:
  -h, --help     Show help message
  -d, --daemon   Run as daemon (background process)
  -s, --stop     Stop any running monitor daemon
  -c, --check    Check dependencies and configuration
  -b, --build    Build documentation once and exit

Examples:
  ./monitor-docs.sh              # Start monitoring (foreground)
  ./monitor-docs.sh --daemon     # Start monitoring as daemon
  ./monitor-docs.sh --build      # Build documentation once
  ./monitor-docs.sh --stop       # Stop daemon
```

### Systemd Service (Advanced)

For production environments, you can install as a systemd service:

```bash
# Install service (replace 'username' with your username)
sudo cp docs-monitor.service /etc/systemd/system/docs-monitor@.service
sudo systemctl daemon-reload

# Enable and start service
sudo systemctl enable docs-monitor@username.service
sudo systemctl start docs-monitor@username.service

# Check status
sudo systemctl status docs-monitor@username.service

# View logs
sudo journalctl -u docs-monitor@username.service -f
```

## Monitored Files and Directories

The system monitors changes in:

### Directories
- `src/` - Source code files
- `docs/` - Documentation files
- `tests/` - Test files
- `api/` - API documentation files

### Files
- `docfx.json` - DocFX configuration
- `README.md` - Main readme file
- `index.md` - Index documentation
- `toc.yml` - Table of contents
- `*.md` - All markdown files

### File Types Watched
- `*.cs` - C# source files
- `*.csproj` - C# project files
- `*.md` - Markdown files
- `*.yml`, `*.yaml` - YAML files
- `*.json` - JSON configuration files

## Configuration

### Debounce Settings

The monitor uses a 2-second debounce time to prevent excessive rebuilds. You can modify this in `monitor-docs.sh`:

```bash
DEBOUNCE_TIME=2  # Wait 2 seconds after last change before rebuilding
```

### Exclusions

The following paths are automatically excluded from monitoring:
- `.git/`, `.vs/`, `.vscode/` - Version control and IDE directories
- `bin/`, `obj/` - Build output directories
- `_site/` - DocFX output directory
- `TestResults/` - Test output directory
- `*.tmp`, `*.temp`, `*.swp`, `*.swo`, `*.log` - Temporary files

## Log Files

- **`docs-monitor.log`** - Detailed log of all monitoring activities and build results
- **Console Output** - Real-time status updates with colorized messages

### Log Messages

- 🚀 Starting monitor
- 📝 File change detected
- 🔨 Building documentation
- ✅ Build completed successfully
- ❌ Build failed
- 🔄 DocFx serve detected (auto-reload)
- ⏳ Debouncing changes

## Serving Documentation

### Local Development

Start DocFX serve alongside the monitor:

```bash
# Terminal 1: Start monitoring
./monitor-docs.sh

# Terminal 2: Serve documentation
docfx serve _site
```

The documentation will be available at `http://localhost:8080`

### Auto-reload

When DocFX serve is running, the monitor detects it and your browser will automatically refresh when documentation is rebuilt.

## Troubleshooting

### Common Issues

1. **"inotifywait: command not found"**
   ```bash
   sudo apt-get install inotify-tools
   ```

2. **"docfx: command not found"**
   ```bash
   dotnet tool install -g docfx
   export PATH=$PATH:$HOME/.dotnet/tools
   ```

3. **"dotnet: command not found"**
   ```bash
   ./install-dependencies.sh
   ```

4. **Build failures**
   - Check `docs-monitor.log` for detailed error messages
   - Ensure all C# projects compile: `dotnet build`
   - Validate DocFX configuration: `docfx build docfx.json`

5. **Permission issues**
   ```bash
   chmod +x *.sh
   ```

### Debugging

1. **Check system status**:
   ```bash
   ./monitor-docs.sh --check
   ```

2. **Test build manually**:
   ```bash
   ./monitor-docs.sh --build
   ```

3. **View detailed logs**:
   ```bash
   tail -f docs-monitor.log
   ```

4. **Test file monitoring**:
   ```bash
   # In another terminal, make a test change
   echo "test" >> docs/test.md
   # Watch for detection in monitor output
   ```

## Advanced Configuration

### Custom Build Script

You can modify `build-docs.sh` to customize the build process:

```bash
# Add custom pre-build steps
echo "Running custom pre-build tasks..."

# Add custom post-build steps
echo "Running custom post-build tasks..."
# Example: Copy additional files, run validation, etc.
```

### Custom Monitoring

Modify the `WATCH_DIRS` and `WATCH_FILES` arrays in `monitor-docs.sh`:

```bash
WATCH_DIRS=(
    "src/"
    "docs/"
    "tests/"
    "api/"
    "custom-dir/"  # Add custom directory
)

WATCH_FILES=(
    "docfx.json"
    "README.md"
    "custom-config.yml"  # Add custom file
)
```

## Performance Considerations

- **Large Codebases**: The monitor is efficient but may use more CPU/memory with very large repositories
- **Network Drives**: Monitoring files on network drives may be slower
- **Docker**: When running in Docker, ensure proper volume mounting for file change detection

## Security

The systemd service runs with restricted permissions:
- No new privileges
- Private temporary directories
- Read-only home directory access
- Kernel protection enabled

## Integration

### CI/CD Integration

Add to your CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Build Documentation
  run: ./monitor-docs.sh --build

- name: Deploy Documentation
  run: |
    # Deploy _site/ directory to your hosting platform
```

### IDE Integration

Most IDEs will work seamlessly with the monitor. File changes from:
- Visual Studio / VS Code
- JetBrains Rider
- vim/emacs
- Any text editor

Will be automatically detected and trigger rebuilds.

## Support

If you encounter issues:

1. Check this documentation
2. Review log files
3. Test dependencies with `--check`
4. Try a manual build with `--build`
5. Check file permissions on scripts