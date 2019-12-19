## docker network


* 创建网络连接
```
docker network create -d bridge net_mysql
```

## docker run

* -d 后台运行容器，并返回容器ID

* --name="nginx-lb" 为容器指定一个名称

* -e username="ritchie" 设置环境变量；

* -m 设置容器使用内存最大值

* -p 指定端口映射，格式为：主机(宿主)端口:容器端口

## docker exec 

通过 exec 命令对指定的容器执行 bash

```
docker exec -it 9df70f9a0714 /bin/bash
```
