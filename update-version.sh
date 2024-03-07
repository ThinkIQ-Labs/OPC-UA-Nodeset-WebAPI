#!/bin/bash

# new test build

# verify git token is supplied as argument
echo Checking for args..
if [ $# -eq 0 ]; then
    echo No arg provided.. Please provide Git Token
    exit 1
fi

# get tag from codebuild env
tag=$CODEBUILD_WEBHOOK_TRIGGER

echo codebuild triggered tag: $tag

# would like to find a way to stop the debug build if there is a release tag

#if [[ $TAG_NAME =~ "^\d+\.\d+\.\d+$" ]]; then
#    echo "tag $TAG_NAME is correctly formated"
#else
#    echo "tag $TAG_NAME is not correctly formated. using branch name and current date"
#    #file_suffix=${TAG_NAME}_$(date +%Y-%m-%d)
#    file_suffix=${TAG_NAME}
#fi

tag_name=${tag##*/}
echo tag name: $tag_name

if [[ $tag == *"releases"* ]]; then
    tag=$tag_name
    echo build has associated tag: $tag
    echo 'export BUILD_CONFIG_TYPE="Release"' >> build-artifacts.sh
else
    echo development build has no tag, using: $tag_name
    tag=$tag_name
    echo 'export BUILD_CONFIG_TYPE="Debug"' >> build-artifacts.sh
fi

echo "export TAG_NAME=${tag_name}" >> build-artifacts.sh

# header build for github api
echo building headers for github api
authentication="Authorization: Bearer ${1}"
#githubapi_all_tags="https://api.github.com/repos/ThinkIQ/opc-ua-nodeset-webapi/tags"
accept="Accept: application/vnd.github+json"
githubapiversion="X-GitHub-Api-Version: 2022-11-28"

# get tag list from github for repo
#response=$(curl -L -H "${accept}" -H "${authentication}" -H "${githubapiversion}" ${githubapi_all_tags})

timestamp=$(date "+%Y-%m-%d %H:%M:%S %Z")
commit=$CODEBUILD_SOURCE_VERSION
buildcomputer=$HOSTNAME

echo $timestamp
echo $commit
echo $buildcomputer

echo "update version-info.sh"
sed -i s/COMMIT-REPLACE/${commit}/ version-info.sh
sed -i s/TAG-REPLACE/${tag_name}/ version-info.sh
sed -i s/VERSION-REPLACE/${tag_name}/ version-info.sh
sed -i s/HOSTNAME-REPLACE/${buildcomputer}/ version-info.sh
sed -i s/TIMESTAMP-REPLACE/"${timestamp}"/ version-info.sh

echo "finished version update:"
cat version-info.sh
