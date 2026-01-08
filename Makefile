.PHONY: dev api web build test clean

# Run everything in parallel
dev:
	@echo "Starting Preznt development servers..."
	@make -j2 api web

# Run API only
api:
	cd apps/api/src/Preznt.Api && dotnet watch run

# Run Web only
web:
	cd apps/web && npm run dev

# Build all
build:
	cd apps/api/src/Preznt.Api && dotnet build
	cd apps/web && npm run build

# Test all
test:
	cd apps/api && dotnet test
	cd apps/web && npm test

# Clean build artifacts
clean:
	cd apps/api && dotnet clean
	cd apps/web && rm -rf dist node_modules

# Install dependencies
install:
	cd apps/api && dotnet restore
	cd apps/web && npm install

# Generate API client from OpenAPI spec
generate-client:
	npx @openapitools/openapi-generator-cli generate \
		-i contracts/openapi.yaml \
		-g typescript-fetch \
		-o contracts/generated
