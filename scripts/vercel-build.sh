# scripts/vercel-build.sh
#!/usr/bin/env bash
set -euo pipefail

# 1) Install the .NET SDK that Vercel's build container doesn't have
curl -sSL https://dot.net/v1/dotnet-install.sh \
     | bash /dev/stdin -c 8.0 -InstallDir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# 2) Publish the solution as static files
dotnet publish SportsAnalysis.sln \
    -c Release \
    -p:PublishTrimmed=true \
    -o build

# 3) Move static assets where Vercel expects them
mkdir -p .vercel/output/static
mv build/wwwroot/* .vercel/output/static/

# 4) Mark the deployment as “static only”
echo '{ "version": 3 }' > .vercel/output/config.json
