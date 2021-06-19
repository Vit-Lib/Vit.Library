set -e


#---------------------------------------------------------------------
#(x.1)参数
args_="

export codePath=/root/temp/svn

export version=`grep '<Version>' "${codePath}" -r --include Vit.Core.csproj | grep -oP '>(.*)<' | tr -d '<>'`

export export GIT_SSH_SECRET=xxxxxx

export name=Vit.Library


# "

 



#----------------------------------------------
echo "(x.2.1)发布文件-创建文件夹及内容"

mkdir -p $codePath/Publish/release/${name}-${version}

 

echo 1.复制 nuget
\cp -rf $codePath/Publish/Publish/nuget/. $codePath/Publish/release/${name}-${version}/nuget


echo "(x.2.3)发布文件-压缩" 
docker run --rm -i \
-v $codePath:/root/code \
serset/filezip dotnet FileZip.dll zip -p -i /root/code/Publish/release/${name}-${version} -o /root/code/Publish/release/${name}-${version}.zip





#----------------------------------------------
echo "(x.3)github-提交release文件到release仓库"
# releaseFile=$codePath/Publish/release/${name}-${version}.zip

#复制ssh key
echo "${GIT_SSH_SECRET}" > $codePath/Publish/release/serset
chmod 600 $codePath/Publish/release/serset

#推送到github
docker run -i --rm \
-v $codePath/Publish/release:/root/release serset/git-client bash -c "
set -e
ssh-agent bash -c \"
ssh-add /root/release/serset
ssh -T git@github.com -o StrictHostKeyChecking=no
git config --global user.email 'serset@yeah.com'
git config --global user.name 'lith'
mkdir -p /root/code
cd /root/code
git clone git@github.com:serset/release.git /root/code
mkdir -p /root/code/file/${name}
cp /root/release/${name}-${version}.zip /root/code/file/${name}
git add file/${name}/${name}-${version}.zip
git commit -m 'auto commit ${version}'
git push -u origin master \" "





 
 
