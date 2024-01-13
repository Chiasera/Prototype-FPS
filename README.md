# CollisionDetection
 Implementation of collision detection without built in physics engine

About the assets. All shaders have been made my myself, some modified for the purpose of this assignment, some from some time ago. 
The textures are not all mine, as they come from the asset store (whether bought or free), especially the stylized tiled textures and normal maps from Cartoon_Texture_Pack.
Here are my inspirations from some years ago for the water shader that I have been updating:
* https://www.youtube.com/watch?v=gRq-IdShxpU&ab_channel=Unity
* https://www.youtube.com/watch?v=kGEqaX4Y4bQ&ab_channel=JumpTrajectory
* https://www.youtube.com/watch?v=MHdDUqJHJxM&t=17s&ab_channel=BinaryLunar

1)Game is FPS, all camera controls except the camera shake has been code by myself.

2) Starting area : The player is spawned in the corner of the terrain. It is pretty obvious they have to cross the bridge to get to the other side. 
Grass surrounds the main path that leads to the goal.

3)The briges sink into the wate after the player picks up the weapon and is able to shoot. The AIs wait for some time before shooting back at the player.
One hit is enough to kill them. Bullets autodestruct if certain height is reached or after a given amount of seconds. The player continue along the path while shooting
at enemies and dodge the bullets.

4) Canyon area: once the player hits all the targets, he can trigger the first floating stone to rise. Once they step on it, two more pop up, if they continue,
the player can no longer go back as previous stones dissolve on their way. The path generated is like a tree with a branching factor of 2. When the player reaches 
a leaf, the parent stone and the sibling are dissolved. One out of the two generated stones at each iteration is "weak". This is chosen randomly. If the player
steps on a weak stone, it dissolved quickly and the player has little time to continue on its way. It creates pressure. However, the weak tiles hover slightly differently
than the fixed ones, so which path to take should be obvious. 

5) Once the player crosses the canyon a slightly glittering transparent violet wall awaits them and once triggered, the win screen appears, congratulations !
