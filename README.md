# 🌩️💨 Nimbus
Compatibility patches for the Resonite Headless server to enable greater .NET 8 compatibility

- Ensures that types sent to the client are compatible, allowing normal clients to join
- Patches types for saving to ensure that the Headless can save worlds
- Wraps and re-routes deprecated thread options to perform safer shutdown of background jobs

# Installation
Simply download the dll provided in the releases section of this repo and move it to the rml_mods folder on your Headless server.

**HIGHLY RECOMMENDED:** Download the companion pre-patcher [Cumulo](https://github.com/RileyGuy/Cumulo) to install the required tweaks, which will also install the latest bundled version of Nimbus alongside it. The patcher is required and is an all-in-one solution to grabbing everything you need to run the Headless server on .NET 8.