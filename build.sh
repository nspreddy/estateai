#!/bin/bash
set -e
start=${SECONDS}
arg=$1

go version
env

# This allows the go tools to find custom packages placed at the project root
mkdir -p ${GOPATH}/src/nrideas.visualstudio.com/
ln -s $PWD ${GOPATH}/src/nrideas.visualstudio.com/
cd ${GOPATH}/src/nrideas.visualstudio.com/EstateAI

make local 
make docker-prep
