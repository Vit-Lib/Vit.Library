/*
 * vit.ChunkUpload 分块文件上传器
 * Date   : 2021-06-03
 * Version: 1.0.5
 * author : Lith
 * email  : serset@yeah.net
 */
; (function (vit) {


    function Deferred() {
        var self = this;


        function createCallback(eventName, fireName) {

            var eventArray = [];
            self[eventName] = function (event) {
                eventArray.push(event);
                return self;
            };

            self[fireName] = function () {
                for (var event of eventArray) {
                    event.apply(self, arguments);
                }
                return self;
            };
        }

        self.reset = function () {
            createCallback('onStart', 'fireStart');
            createCallback('beforeUploadChunk', 'fireBeforeUploadChunk');

            createCallback('progress', 'notify');
            createCallback('done', 'resolve');
            createCallback('fail', 'reject');

            return self;
        };

        self.reset();

    }



    vit.ChunkUpload = function () {
        var self = this;

        //var deferred = self.deferred = $.Deferred();
        var deferred = self.deferred = new Deferred();


        //文件块大小,默认 102400
        self.chunkSize = 102400;

        //上传文件的地址
        self.url;

        var fileGuid;
        self.file;
        self.uploadedSize;

        // 空闲  上传中  用户终止
        var uploadState='空闲';

        var event_onchange;
        function onchange() {

            if (this.files.length != 1) return;

            self.setFile(this.files[0]); 

            $(this).remove();

            if (event_onchange) event_onchange();
        }


        self.setFile=function(file){ 
            if(uploadState!='空闲')throw new Error("不可在上传过程中修改文件");
            self.file = file;
        };


        self.selectFile = function (_event_onchange) {
            event_onchange = _event_onchange;
            $('<input type="file"  >').change(onchange).click();
        };


        //开始上传
        self.startUpload = function () {
            if(uploadState!='空闲')throw new Error("不可在上传过程中开始新的上传任务");
            uploadState='上传中';

            let file = self.file;
            self.uploadedSize = 0;
            fileGuid = '' + file.size + '_' + file.name + '_' + Math.random();
            deferred.fireStart(file, fileGuid);
            uploadChunk_start();
        };

        //终止上传
        self.stopUpload=function(){
            if(uploadState=='上传中'){
                uploadState='用户终止';
                return true;
            }              
            //throw new Error("只可在上传过程中终止上传任务");    
            return false;      
        };

        function uploadChunk_start() {
            let file = self.file;


            //获取文件块的位置
            let end = (self.uploadedSize + self.chunkSize > file.size) ? file.size : (self.uploadedSize + self.chunkSize);

            //将文件切块上传
            let fd = new FormData();
            fd.append('_ChunkUpload_files', file.slice(self.uploadedSize, end), file.name);

            var chunkInfo = {
                fileGuid: fileGuid,
                startIndex: self.uploadedSize,
                fileSize: file.size
            };

            fd.append("_ChunkUpload_fileGuid", chunkInfo.fileGuid);
            fd.append("_ChunkUpload_startIndex", chunkInfo.startIndex);
            fd.append("_ChunkUpload_fileSize", chunkInfo.fileSize);

            self.uploadedSize = end;

            var ajaxConfig = {
                url: self.url,
                type: 'post',
                data: fd,
                processData: false,
                contentType: false,
                success: uploadChunk_onSuccess
            };

            deferred.fireBeforeUploadChunk(ajaxConfig);

            //POST表单数据
            $.ajax(ajaxConfig);
        }

        function uploadChunk_onSuccess(apiRet) {
            let file = self.file;

            //上传失败
            if (!apiRet || apiRet.success != true || uploadState=='用户终止') {
                deferred.reject(apiRet);
                uploadState='空闲';
                return;
            }

            //上传完成
            if (self.uploadedSize >= file.size) {
                deferred.resolve(apiRet);
                uploadState='空闲';
                return;
            }

            //更新进度
            deferred.notify(self.uploadedSize, file.size);



            //继续上传下一文件块
            uploadChunk_start();
        }


    };


})('undefined' === typeof (vit) ? vit = {} : vit);


/*
//demo:
var chunkUpload = new vit.ChunkUpload();

//文件块大小,默认 102400
chunkUpload.chunkSize = 1 * 1024 * 1024;

//上传文件的地址
chunkUpload.url = '/upload/uploadchunk';

chunkUpload.deferred
    .reset()
    .beforeUploadChunk(function (ajaxConfig) {
        var formData = ajaxConfig.data;
        if (formData.arg) return;
        formData.arg = true;
        formData.append("type", 'test');
    })
    .onStart(function (file, fileGuid) {
        console.log('开始上传，文件名：' + file.name + '  文件大小：' + (file.size / 1024.0 / 1024.0).toFixed(3) + ' MB');
    })
    .progress(function (uploadedSize, fileSize) {
        console.log('已经上传：' + (uploadedSize / 1024.0 / 1024.0).toFixed(3) + ' MB  百分比：' + (uploadedSize / fileSize * 100).toFixed(2) + '%');
    })
    .done(function (apiRet) {
        console.log("上传成功！");
        console.log(apiRet);
    })
    .fail(function (apiRet) {
        console.log("上传出错！");
        console.log(apiRet);
    });

//chunkUpload.selectFile();
//var file = chunkUpload.file;
//chunkUpload.startUpload();

//选择文件 随后立即开始上传
chunkUpload.selectFile(function () { chunkUpload.startUpload(); });

//终止文件上传
chunkUpload.stopUpload();

//*/