docker run -d -p 2717:27017 -v %cd%/Database:/data/db --name mymongocr mymongo:1.0.3
::ping 127.0.0.1 > nul
docker exec -it mymongocr bash
docker rm -f mymongocr
PAUSE
