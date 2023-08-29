#!/bin/bash -eu

function running_as_root
{
    test "$(id -u)" = "0"
}

# Parameters must separated when using ARM
# This won't be set yet then
if [[ -z "${NEO4J_AUTH}" ]]; then
    echo "NEO4J_AUTH not present. Building it with N4J_USER and N4J_PW..."
    export NEO4J_AUTH="${N4J_USER}/${N4J_PW}"
fi

# Fork the original EP which will start the actual Neo4J service
/startup/docker-entrypoint.sh "$@" &

# Poll for Neo4J to be ready to receive commands
set +e
declare -i neo4j_started=1
until [ $neo4j_started -eq 0 ]; do
    echo "Waiting on Neo4J to start..."
    sleep 20
    pidof -s java &>/dev/null
    neo4j_started=$(($neo4j_started & $?))
    ss -plntu | grep :7687 &>/dev/null
    neo4j_started=$(($neo4j_started | $?))
    ss -plntu | grep :7474 &>/dev/null
    neo4j_started=$(($neo4j_started | $?))
done
set -e

echo "Getting credentials..."
IFS='/' read -r cs_user cs_password <<<"${NEO4J_AUTH}"

echo "Executing setup scripts..."
if running_as_root; then
    cat /opt/matts/constraints.cypher | gosu neo4j:neo4j cypher-shell -a neo4j://0.0.0.0:7687 -u $cs_user -p $cs_password --format plain
    # Put additional scripts here
else
    cat /opt/matts/constraints.cypher | cypher-shell -a neo4j://0.0.0.0:7687 -u $cs_user -p $cs_password --format plain
    # Put additional scripts here
fi

echo "Initial Setup Complete!"
# Pause bash instance running this script so the container doesn't exit
kill -STOP $$
