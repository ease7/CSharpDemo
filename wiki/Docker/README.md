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

## docker system

docker system prune命令可以用于清理磁盘，删除关闭的容器、无用的数据卷和网络，以及dangling镜像（即无tag的镜像）。docker system prune -a命令清理得更加彻底，可以将没有容器使用Docker镜像都删掉。注意，这两个命令会把你暂时关闭的容器，以及暂时没有用到的Docker镜像都删掉了

```
docker system prune -a
```

查看Docker的磁盘使用情况

```
docker system df
```

## 挂载卷

挂载图片文件夹

```
docker run -d --name test \
-p 8001:80 \
-v /usr/imgs:/app/wwwroot/imgs \
test
```

