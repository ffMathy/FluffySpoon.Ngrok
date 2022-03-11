using System.Diagnostics;
using FluffySpoon.AspNet.Ngrok.Sample;

var app = Startup.Create();
await app.RunAsync();