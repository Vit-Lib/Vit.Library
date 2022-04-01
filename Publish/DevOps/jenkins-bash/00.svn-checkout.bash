
#(x.1)参数
basePath=/root/docker-data/dev/jenkins/jenkins_home/workspace/Repo/Vit.Library
export SVN_PATH=svn://svn.ki.lith.cloud/2020LithProject/Library/Vit.Library
export SVN_USERNAME=jenkins
export SVN_PASSWORD=xxxxxx


#(x.2)创建文件夹
mkdir -p $basePath/code
chmod 777 $basePath/code



#(x.3)从svn拉取code
docker run -it --rm -v $basePath/code:/root/svn serset/svn-client svn checkout "$SVN_PATH" /root/svn --username "$SVN_USERNAME" --password "$SVN_PASSWORD" --no-auth-cache

