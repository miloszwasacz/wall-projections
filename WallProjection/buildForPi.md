### To build and run for a raspberry pi, run in terminal:

- `dotnet build`
- `dotnet publish --runtime linux-arm --self-contained`

This will create a folder in /bin/Debug/net6.0 called linux-arm

- Move the linux-arm folder onto the pi
- Inside the linux-arm folder there is an executable called WallProjection
- Give yourself execute access to the file `sudo chmod +x WallProjection`
- Then exectute the file `./WallProjection`
- It will take a moment to load but a window should pop up

