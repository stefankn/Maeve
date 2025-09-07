if [ -f "aspirate-output/docker-compose.yaml" ]; then
   docker compose -f aspirate-output/docker-compose.yaml down
fi
 
~/.dotnet/tools/aspirate generate --compose-build finch-mcp-server --compose-build hue-mcp-server --output-format compose
docker compose -f aspirate-output/docker-compose.yaml --no-cache build
docker compose -f aspirate-output/docker-compose.yaml up