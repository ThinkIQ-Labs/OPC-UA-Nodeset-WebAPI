#!/bin/bash

# used to store env variables for use during build across different scripts

githubapi_all_tags="https://api.github.com/repos/ThinkIQ-Labs/opc-ua-nodeset-webapi/tags"

GIT_REL_API="https://api.github.com/repos/ThinkIQ-Labs/opc-ua-nodeset-webapi/releases"
GIT_TAG_API="https://api.github.com/repos/ThinkIQ-Labs/opc-ua-nodeset-webapi/releases/tags"
GIT_UPLOAD_API="https://uploads.github.com/repos/ThinkIQ-Labs/opc-ua-nodeset-webapi/releases"
S3_DEV_PATH="s3://tiq-codebuild/opc-ua-nodeset-webapi/DEVELOPMENT"
S3_REL_PATH="s3://tiq-codebuild/opc-ua-nodeset-webapi/RELEASED"

server_zip_prefix="nodeset_api"
server_build_dir="BuildOutput/"
