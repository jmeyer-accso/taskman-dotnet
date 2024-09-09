#!/bin/bash

docker run --rm -d -p 5432:5432 \
    --name taskman-db \
    -e POSTGRES_PASSWORD='t@skm@n123' \
    -e POSTGRES_USER=taskman \
    -e POSTGRES_DB=taskman \
    postgres:latest
