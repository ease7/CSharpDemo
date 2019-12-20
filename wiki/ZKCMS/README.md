

# 纸壳CMS


## Docker 发布


```
docker run -d --name zkcms001 \
--network net_mysql \
-p 8001:80 \
-e zkdb='server=mysql002;port=3306;user=root;password=xxxx;database=ZKEACMS;' \
eseven/zkcms:mysql

```

## Docker 发布2

```
docker run -d --name zkcms001 \
-p 8001:80 \
-e zkdb='server=172.17.0.1;port=3306;user=root;password=xxxx;database=ZKEACMS;' \
eseven/zkcms:mysql

```