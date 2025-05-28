RUN docker build -f ./src/TtWork.Project.Web.Host/Dockerfile --network=host --build-arg HTTP_PROXY=http://192.168.3.50:10809 --build-arg HTTPS_PROXY=http://192.168.3.50:10809 -t gitlab.somall.top:8090/molitao/api:latest .; docker push gitlab.somall.top:8090/molitao/api:latest

@pause