if [ -f "maeve/docker-compose.yaml" ]; then
   docker compose -f maeve/docker-compose.yaml down
fi
 
~/.dotnet/tools/aspirate generate --compose-build finch-mcp-server --compose-build hue-mcp-server --output-format compose --output-path maeve

# START hacks

# Cannot specify restart policy, migrations service should not be restarted when completed.
sed -i.tmp '/^  migrations:/,/^  [a-zA-Z]/ {
    /^  [a-zA-Z]:/!{
        s/restart: unless-stopped/restart: no/
    }
}' maeve/docker-compose.yaml

# Add healthcheck to postgres service
sed -i.tmp '/^  postgres:/,/^  [a-zA-Z]/ {
    /restart: unless-stopped/i\
    healthcheck:\
      test: ["CMD-SHELL", "pg_isready -U postgres"]\
      interval: 10s\
      timeout: 5s\
      retries: 5
}' maeve/docker-compose.yaml

# Add depends_on for postgres to migrations service
sed -i.tmp '/^  migrations:/,/^  [a-zA-Z]/ {
    /environment:/i\
    depends_on:\
      postgres:\
        condition: service_healthy
}' maeve/docker-compose.yaml

# Add depends_on to maeve service with migrations completion condition
sed -i.tmp '/^  maeve:/,/^  [a-zA-Z]/ {
    /environment:/i\
    depends_on:\
      migrations:\
        condition: service_completed_successfully
}' maeve/docker-compose.yaml

rm maeve/docker-compose.yaml.tmp

# END hacks

docker compose -f maeve/docker-compose.yaml build
docker compose -f maeve/docker-compose.yaml up -d