import requests
import os
import stat
import urllib
import zipfile
import shutil
import psutil
import subprocess

def main():
    print('Getting latest build id.')
    buildId = getLatestBuildId()
    print(f'Latest build id is {buildId}.')
    print('Checking if local app is up-to-date.')
    
    if not isUpdateNecessary(buildId):
        print('Local app is up-to-date.')
        return

    print('Downloading latest build\'s artifact.')
    url = getArtifactDownloadUrl(buildId)

    cleanAndShutdownUploadR()
    print('UploadR instance has been killed and removed.')
    print('Downloading and installing UploadR')
    downloadAndUnzipLatestBuild(url, buildId)
    print('Authorizing and starting UploadR')
    startUploadR()
    print('OK!')
    quit()

def getLatestBuildId():
    buildsRoute = 'https://dev.azure.com/allanmercou/uploadr/_apis/build/builds?api-version=4.1'
    response = requests.get(url = buildsRoute)
    if not response.ok:
        raise Exception('Response from buildsRoute request failed.')
    
    jsonData = response.json()
    return jsonData['value'][0]['id']

def isUpdateNecessary(buildId):
    verFile = open('./uploadr_build_ver', 'r')
    localBuildId = int(verFile.read())
    verFile.close()

    newVerFile = open('./uploadr_build_ver', 'w')
    newVerFile.write(str(buildId))
    newVerFile.close()
    
    return localBuildId < buildId

def getArtifactDownloadUrl(buildId):
    artifactRoute = f'https://dev.azure.com/allanmercou/uploadr/_apis/build/builds/{buildId}/artifacts?artifactName=uploadr&api-version=4.1'
    response = requests.get(url = artifactRoute)
    if not response.ok:
        raise Exception('Response from artifactRoute request failed.')

    jsonData = response.json()
    return jsonData['resource']['downloadUrl']

def cleanAndShutdownUploadR():
    processName = 'uploadr'
    for processus in psutil.process_iter():
        if processus.name() == processName:
            processus.kill()

def downloadAndUnzipLatestBuild(downloadUrl, buildId):
    download = urllib.request.urlopen(downloadUrl)
    output = open(f'uploadr_build_{buildId}.zip', 'wb')
    output.write(download.read())
    output.close()

    zippedFile = zipfile.ZipFile(f'uploadr_build_{buildId}.zip', 'r')
    zippedFile.extractall()
    zippedFile.close()

def startUploadR():
    os.chdir('./uploadr/UploadR')
    path = './UploadR'
    permissions = os.stat(path)
    os.chmod(path, permissions.st_mode | stat.S_IEXEC)
    subprocess.call(['nohup', path, '&'])
    
if __name__ == '__main__':
    main()
