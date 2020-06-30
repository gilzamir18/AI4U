#!/bin/sh

if [ "$#" -ne 3 ]; then
    echo "Usage: $0 AGENT LOGS_DIRECTORY LOG_DIRECTORY" >&2
    exit 1
fi
AGENT=$1
CKP_PATH=$2/$3/checkpoints/

python3 $AGENT --run test --id 0 --path $CKP_PATH &
python3 $AGENT --run test --id 1 --path $CKP_PATH &
python3 $AGENT --run test --id 2 --path $CKP_PATH &
python3 $AGENT --run test --id 3 --path $CKP_PATH &
python3 $AGENT --run test --id 4 --path $CKP_PATH &
python3 $AGENT --run test --id 5 --path $CKP_PATH &
python3 $AGENT --run test --id 6 --path $CKP_PATH &
python3 $AGENT --run test --id 7 --path $CKP_PATH &