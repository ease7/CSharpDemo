

# 纸壳CMS

## docker 发布

### 创建网络连接

```bash
docker network create -d bridge net_mysql
```

### 安装mysql

```bash
docker run -d --name mysql002 \
--network net_mysql \
--rm \
-p 3307:3306 \
-e MYSQL_ROOT_PASSWORD=root mysql
```



### Docker 发布(network)


```bash
docker run -d --name zkcms001 \
--network net_mysql \
-p 8001:80 \
-e zkdb='server=mysql002;port=3306;user=root;password=root;database=ZKEACMS;' \
eseven/zkcms:1.0.2
```

### Docker 发布

```bash
docker run -d --name zkcms001 \
-p 8001:80 \
-v /usr/imgs:/app/wwwroot/imgs \
-e zkdb='server=172.17.0.1;port=3306;user=root;password=xxxx;database=ZKEACMS;' \
eseven/zkcms:1.0.0
```