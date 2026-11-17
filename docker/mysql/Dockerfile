FROM mysql:8.4

COPY docker/mysql/local-entrypoint.sh /usr/local/bin/local-entrypoint.sh
RUN chmod +x /usr/local/bin/local-entrypoint.sh

ENTRYPOINT ["/usr/local/bin/local-entrypoint.sh"]
CMD ["mysqld"]
