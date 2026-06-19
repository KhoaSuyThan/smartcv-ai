import urllib.request

try:
    response = urllib.request.urlopen("http://localhost:5170", timeout=5)
    print("Status code:", response.getcode())
    print("Headers:", response.info())
except Exception as e:
    print("Error:", e)
