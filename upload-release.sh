#!/bin/bash

# verify git token is supplied as argument
echo Checking for args..
if [ $# -eq 0 ]; then
    echo No arg provided.. Please provide Git Token
    exit 1
fi

#GIT_REL_API="https://api.github.com/repos/ThinkIQ/opc-ua-nodeset-webapi/releases"
#GIT_TAG_API="https://api.github.com/repos/ThinkIQ/opc-ua-nodeset-webapi/releases/tags"
#GIT_UPLOAD_API="https://uploads.github.com/repos/ThinkIQ/opc-ua-nodeset-webapi/releases"
#S3_DEV_PATH="s3://tiq.builds/opc-ua-nodeset-webapi/DEVELOPMENT"
#S3_REL_PATH="s3://tiq.builds/opc-ua-nodeset-webapi/RELEASED"

BuildNumber=""
GitReleaseId=""
IsReleaseBuild="false"

if [[ $BUILD_CONFIG_TYPE == "Release" ]]; then
    echo "tag $TAG_NAME is correctly formated. using generic name for versioning."
    server_zip_name="${server_zip_prefix}.zip"
else
    echo "tag $TAG_NAME is not correctly formated. using branch name and current date"
    #file_suffix=${TAG_NAME}_$(date +%Y-%m-%d)
    file_suffix=${TAG_NAME}
    server_zip_name="${server_zip_prefix}_${file_suffix}.zip"
fi

#server_zip_name="opcuaserver_${file_suffix}.zip"
#server_build_dir="BuildOutput/"

cp version-info.sh $server_build_dir
cd $server_build_dir
pwd
zip -r $server_zip_name .

if [[ $BUILD_CONFIG_TYPE == "Release" ]]; then
    aws s3 cp ./$server_zip_name $S3_REL_PATH/$server_zip_name
elif [[ $BUILD_CONFIG_TYPE == "Debug" ]]; then
    aws s3 cp ./$server_zip_name $S3_DEV_PATH/$server_zip_name
else
    echo "build config type not set properly"
    exit 1
fi
