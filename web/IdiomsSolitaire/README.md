## 成语接龙小游戏

数据源：mysql

### 数据获取

本数据来源于网络，总共31852条数据，不能说很全，基本包含大部分的成语。

### Docker的方式发布

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