#!/bin/sh
set -eu

install -d -o mysql -g mysql -m 0700 /run/mysql-certs
install -o mysql -g mysql -m 0644 /etc/mysql/certs-source/ca.pem /run/mysql-certs/ca.pem
install -o mysql -g mysql -m 0644 /etc/mysql/certs-source/server-cert.pem /run/mysql-certs/server-cert.pem
install -o mysql -g mysql -m 0600 /etc/mysql/certs-source/server-key.pem /run/mysql-certs/server-key.pem

exec /usr/local/bin/docker-entrypoint.sh "$@"
