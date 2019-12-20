## 成语接龙小游戏

数据源：mysql

## 数据获取

本数据来源于网络，总共31852条数据，不能说很全，基本包含大部分的成语。

## 发布Docker

服务器系统：ubuntu 18.04.2

Docker Version: 19.03.5

* 创建网络连接

```
docker network create -d bridge net_mysql
```

* 拉取mysql镜像

```
docker pull mysql
```

* 启动mysql, 并加入网络连接

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

## Docker发布（mysql在服务器上）

服务器系统：ubuntu 18.04.2

Docker Version: 19.03.5

MySql version: mysql  Ver 14.14 Distrib 5.7.28, for Linux (x86_64) using  EditLine wrapper


环境描述：mysql在服务器主机，应用发布在docker容器上，所以需要容器访问宿主机的mysql服务

更新最新镜像

```
docker pull eseven/chengyu
```

因为服务器上有安装mysql，所以需要连接服务上的mysql,查看docker0的网络连接信息

```
ifconfig
```

启动应用容器

```
docker run -d --name test001 \
-p 5000:5000 \
-e db='server=172.17.0.1;port=3306;user=root;password=123456;database=test;' \
eseven/chengyu
```

### 常见问题

端口占用
```
sudo lsof -i:5000
```


