.PHONY: build-local
build-local:
	docker-compose -f EvidenceApi/compose.yml build

.PHONY: build-test
build-test:
	docker-compose -f EvidenceApi.Tests/compose.yml build

.PHONY: serve-local
serve-local:
	make build-local && make local

.PHONY: serve-test
serve-test:
	make build-test && make test

.PHONY: shell
shell:
	docker-compose run evidence-api bash

.PHONY: local
local:
	docker compose -f EvidenceApi/compose.yml stop
	docker compose -f EvidenceApi/compose.yml up -d

.PHONY: test
test:
	docker compose -f EvidenceApi.Tests/compose.yml stop
	docker compose -f EvidenceApi.Tests/compose.yml run --rm test
	docker compose -f EvidenceApi.Tests/compose.yml stop

.PHONY: stop-local
stop-local:
	docker compose -f EvidenceApi/compose.yml stop

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=test-database -a)
	-docker rm $$(docker ps -q --filter ancestor=test-database -a)
	docker rmi test-database
	docker-compose up -d test-database
