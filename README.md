# Redis pub/sup boardcast data with signalr

### Note
It can use the StackExchangeRedis reference because ServiceStack Redis blocks connections after an hourly 6000 request.

## Installation

for redis
```bash
docker run --name my-redis-container -p 6379:6379 -d redis
```
for redis gui
```bash
docker run -v redisinsight:/db -p 8001:8001 redislabs/redisinsight
```
## Usage

### Producer

### Subscriber

### Signalr
