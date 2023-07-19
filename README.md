# FlatFury
#### FlatFury 2D Unity Multiplayer Game (Test).

Target OS: Android (but also can target PC).

Required version: Unity 2021.3.9f, Visual Studio 2019

#### Brief:
This was a test assignment which I have extended a bit.
I tried to do my best to make it good (though it doesn't use famous DI or ECS at this time...).
Anyway, that experience is useful! \m/

#### Main features:
- Use of Unity Gaming Services (Lobby, Netcode, Relay);
- So lobby-based multiplayer game up to 4 players (adjustable in code);
- Support for late-joining clients (actually, it waits only for 2 players and then starts a game, others may join later);
- Fake objects to hide the connection imperfections;
- Random unique colors per users. Each projectile and unit are colored in own player colors.
- Supports touchscreen, gamepad, keyboard and mouse controls (mouse separately).

#### Core Gameplay:
- Collect coins while attacking your enemies... Or just attack your enemies! Forget about coins, toss them to your witcher!

###### P.S. Game contains just visuals, no sounds.

###### P.P.S. My "Netcode for Gameobjects" package had a weird bug: it said it's up-to-date but it lacked a couple of functions I required. And if I searched them in another project and Unity version - everything was ok!.. 
###### Examples:
###### - not complete method "MarkNetworkObjectDirty()" in "NetworkList";
###### - missing "SetRelayServerData(RelayServerData serverData)" overload in "UnityTransport";
###### both made it impossible to work, so I had to insert the code by myself and then I hoped it would not rollback...

Regards, Oleg [Knight Rider] Tolmachev.
