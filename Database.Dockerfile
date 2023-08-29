FROM neo4j:5.6.0
ENV N4J_VERSION=5.6.0

# Declared at: https://github.com/neo4j/docker-neo4j/blob/master/docker-image-src/5/coredb/Dockerfile-debian
ENV NEO4J_HOME="/var/lib/neo4j"

COPY --chmod=a+x ./.neo4j/new-docker-entrypoint.sh /startup/new-docker-entrypoint.sh

WORKDIR /opt/matts
ADD https://dist.neo4j.org/cypher-shell/cypher-shell_${N4J_VERSION}_all.deb .
ADD https://download.oracle.com/java/17/latest/jdk-17_linux-x64_bin.deb .
COPY ./.neo4j/constraints.cypher .
RUN apt update \
    && dpkg -i jdk-17_linux-x64_bin.deb \
    && dpkg -i cypher-shell_${N4J_VERSION}_all.deb \
    && apt-get -f -y install \
    && apt-get -y install iproute2

WORKDIR "${NEO4J_HOME}"

VOLUME /data /logs
EXPOSE 7474 7473 7687

# azure can't execute the scripts? hope this fixes it
RUN chmod -R a+x /startup/*.sh

ENTRYPOINT ["tini", "-g", "--", "/startup/new-docker-entrypoint.sh"]
CMD ["neo4j"]
