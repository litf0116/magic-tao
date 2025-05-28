```
docker build -f ./FreeIM/ImServer/Dockerfile --network=host --build-arg HTTP_PROXY=http://192.168.3.50:10809 --build-arg HTTPS_PROXY=http://192.168.3.50:10809 -t gitlab.somall.top:8090/molitao/freeim:latest .;docker push  gitlab.somall.top:8090/molitao/freeim:latest




docker login molitao 91E^lLJT

docker pull gitlab.somall.top:8090/molitao/freeim:latest;docker stop freeim;docker rm freeim;docker run -d --network=app --restart=always -p 6001:6001  -e TZ='Asia/Shanghai' --name freeim gitlab.somall.top:8090/molitao/freeim:latest





```

# API

```
docker build -f ./src/TtWork.Project.Web.Host/Dockerfile --network=host --build-arg HTTP_PROXY=http://192.168.3.50:10809 --build-arg HTTPS_PROXY=http://192.168.3.50:10809 -t gitlab.somall.top:8090/molitao/api:latest .; docker push gitlab.somall.top:8090/molitao/api:latest



docker pull gitlab.somall.top:8090/molitao/api:latest;docker stop api;docker rm api;\
docker run -d --network=app --restart=always -v /www/wwwroot/www.molitao.top/wwwroot:/app/wwwroot -p 5000:5000  -e TZ='Asia/Shanghai' --name api gitlab.somall.top:8090/molitao/api:latest


docker pull gitlab.somall.top:8090/molitao/api:latest;docker stop api ; docker rm api; docker run -d --network=app --restart=always -v /www/certs:/app/cert -v /www/wwwroot/www.molitao.top/wwwroot:/app/wwwroot -p 5000:5000  -e TZ='Asia/Shanghai' --name api gitlab.somall.top:8090/molitao/api:latest


```

```
PH=$(echo 'www_molitao_top' | docker run --rm -i datalust/seq config hash)


docker run \
  --name seq \
  -d \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$PH" \
  -v /www/seq/data:/data \
  -p 5341:80 \
  datalust/seq
```





有人进组队频道发那个组队声



房主踢人

看房间人的列表



![image-20240322183605897](https://cdn.wujiangapp.com/PicGo/202403221836996.png)

```
<xml>
    <ToUserName><![CDATA[gh_7c4b682367f4]]></ToUserName>
    <FromUserName><![CDATA[oFzSV6rZotPwdFx-4pBas6TwPVhc]]></FromUserName>
    <CreateTime>1711276623</CreateTime>
    <MsgType><![CDATA[event]]></MsgType>
    <Event><![CDATA[SCAN]]></Event>
    <EventKey><![CDATA[383296e5-f0a9-4f97-9c74-8a8598e715f1]]></EventKey>
    <Ticket><![CDATA[gQFa8DwAAAAAAAAAAS5odHRwOi8vd2VpeGluLnFxLmNvbS9xLzAyM19yVTBqblFlbEcxM0xROWhDYy0AAgRv_v9lAwSAOgkA]]></Ticket>
    <Encrypt><![CDATA[mWxH+cUPOSbOE39w4n1FRLaiUPEhBKmA6ChcH1AiAuI0JvSyAliv4dF0r7GitykTWlW20YSKFcwUZwnd60SwjImsJyALU/JUjz5xbjmA+TR2nvu78zad4vDgaPsnZZgoYDW7E6F0Of7YG41Hq1c9gajxrqiD4JDIWxYZHw/haFf1HKukfLCLwnOL83yWlYXWF6zv5tk8Tg1VRtf2HpjjzdsU/ZxA0v16ISKKGmuJovDwI0zabzgw37mPduwXkISEDpAFSCU8ylFIALwMRpYfdOFqpLwBlJCQrSZt6QO/TLLx7J2mMpTiOEkqVheCxVw+fvD3lrVUGHWGxvi1G2hIsSXrc9AWGzc1CwkaP9moizF3nmcq8wsK3JBArQcb+44YAH4KiL7cQ2+T0ptBT1TtK/hiJh1wLmnqGfzl218qbnathjGBPLxERY3lB6jDDDlF4MgGB+EokoFPD8Qg8xibHvwN+myATh9dNnDkDw4yYqPxsVi3i9cafuCPhFaPXu3nD2vuSUkHVL4AI6K0FLkIU59onsmTGDlrJnJfv6XHjKN7+FiuIDDEx7iYGbORxtO2uUM8c3zkYEvrz7GUaG0mbxW4vlms6e56LsdeTQZ55H/7Q09osizDedGua/WQkFe3]]></Encrypt>
</xml>

```

```
<xml>
    <ToUserName><![CDATA[gh_7c4b682367f4]]></ToUserName>
    <FromUserName><![CDATA[oFzSV6rZotPwdFx-4pBas6TwPVhc]]></FromUserName>
    <CreateTime>1711276701</CreateTime>
    <MsgType><![CDATA[event]]></MsgType>
    <Event><![CDATA[subscribe]]></Event>
    <EventKey><![CDATA[qrscene_383296e5-f0a9-4f97-9c74-8a8598e715f1]]></EventKey>
    <Ticket><![CDATA[gQFa8DwAAAAAAAAAAS5odHRwOi8vd2VpeGluLnFxLmNvbS9xLzAyM19yVTBqblFlbEcxM0xROWhDYy0AAgRv_v9lAwSAOgkA]]></Ticket>
    <Encrypt><![CDATA[gJ+N2/+usO8s/ikq7Bxs5asPTeWpdLb8GRa6JLpKtSCK5RRjNoQ522Scyukpor4I1fmyCUsW85PxdZTPinn7BSby7Ntt6ipScYfXQcuUnys3jLg9+OhU2XqoQ0ktl0Tit1+0Qe8wm3TI8hsK6qLisKNbEKSZKmj8Tp7j75GTAtQR7Hyx+oS2vD6qGDHtPerhjm8e2bVVYmYiwWsPGzwEE2w4FyM1UIArZLp2tiitnUApKmoHaPvO4y67MUrVTjeoPg+j3NniptUOqmf+x98YMz4DBcEAOJgBdv4aXijrEQnHKzZFJSXgEh+10UrhRiVBoZwrEzfh0/7jxrg5kG0NbvcDgVG+DAOYlGMVU+IR/C3eDEAlL0pwjeSWX30H+K4QPoJEXPKh4jfqy9rwvdXC0i6/j5fnMJ5DsnLB2KA67WpV8P5m/BjZnfa/+Pb4PGEWQMXORhXsZgfs9ktCWihAn4UxEar3K2CIkb0aTrsL/epOlHVR+C5hGwVZykD+fQ/sebHFwHQWmJtvLlONl3Di0jd+xrGulTP4wPUPBC7nAjFac4mz5PQGgCVcCT04cxHQZXxW454FU4hlE7526NR9xQwYy5TgbzxQP5iGgL8eh9yAdgmsqllhWOIuZvoLmjZX9cU2MY7w4o22/9jGZAc4DJjiRNHd/pKErs1ZlTHRAo4=]]></Encrypt>
</xml>


<xml>
    <ToUserName><![CDATA[gh_7c4b682367f4]]></ToUserName>
    <FromUserName><![CDATA[oFzSV6rZotPwdFx-4pBas6TwPVhc]]></FromUserName>
    <CreateTime>1711984635</CreateTime>
    <MsgType><![CDATA[event]]></MsgType>
    <Event><![CDATA[subscribe]]></Event>
    <EventKey><![CDATA[]]></EventKey>
    <Encrypt><![CDATA[C20uRZLbTlxUKENVXPZkEDxJNiMH9npjoCGffADuVGfCHCqX1kZ33LR5UdCyyR5KsVjdM44mTTucDTXwMJ9ZzLD1NyJ2BsEc/SZAhUgc6kVuV5BA/vKz04EC1dDcw25c1cRGvqTP7mLiVhsyo4aEA8At5X99nmqoULJrP6qG8i4aHU6wTNgd05azT0wWt8jeHDR6Vw9A6KTL8wA6i4f7DDvM9ZlYDQyUW/N2g8r1vF+w7Uq+77ODv2JmNwoJwDuyS4WQZ4rQjWkqLHZ/BQEWqLZ9BUzvGBrgMOTEAG/8y1sM1dg3XVSG3YAjEkYUT26X8hJdwsNcaa3t2vtrNz1Ul+dCKytw528oGtq5B8x7/AH1mnjAYpxmF/Dh/DoQSjbT9REyVrLaUPLRLbzfkhL0ovPpP/9HVTYujOMhT1x+fyU=]]></Encrypt>
</xml>

```





```
docker network create app

docker run -d --network=app  --restart=always -p 6379:6379 -v /www/redis/redis.conf:/usr/local/etc/redis/redis.conf  --name redis redis redis-server /usr/local/etc/redis/redis.conf

## redis.conf
bind 0.0.0.0
requirepass xxx

```

