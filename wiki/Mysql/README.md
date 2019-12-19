## 安装镜像

```
docker pull mysql
```

## 启动容器


```
docker run -d --name mysql001 --rm -p 3306:3306 \
-e MYSQL_ROOT_PASSWORD=root mysql

```

## .net core 容器连接mysql容器

创建网络连接

```
docker network create -d bridge net_mysql
```

* 加入网络连接

```
docker run -d --name mysql002 \
--network net_mysql \
--rm \
-p 3307:3306 \
-e MYSQL_ROOT_PASSWORD=root mysql
```

* 启动.net core容器

注意服务器名称为容器名称，端口号为默认的3306

```
docker run -d --name test001 \
--network net_mysql \
-p 5000:5000 \
-e db='server=mysql002;port=3306;user=root;password=root;database=test;' \
chengyu
```

# ubuntu

```
apt install mysql-server

netstat -tap | grep mysql

```

查看状态

systemctl status mysql