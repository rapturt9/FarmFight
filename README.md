# FarmFight

Push Often. Have Fun

Put Game design requests in the Game Design Document comments

Be sure to report issues with other peoples pushes as early as possible

dont be shy about creating tasks if you think someone else needs to do something or you just need a reminder/accountability.

Try Not to commit directly to Main becuase if you mess up we gotta do the work of trying to recover 

if you are unsure about an addition, create a branch and then a pull request

Version control is an acquired skill but i believe in you <3

### I typed this in like 15 mins so Dont hesitate to call me out if i dont make sense or said something wrong



## GitHub Instructions:

### to begin

1. Download git desktop assistant
2. sign in
3. click add/cloneRepository
4. change the local path to a place youll be able to easily access(like a new folder on the desktop)
5. add or (maybe) subtract files as needed



### push, pull, Branches

Here i will talk about version control

A branch is basically an offshoot of the project, 
if you want to make large changes to the project id recommend making a branch and making the changes there
then you can merge the branch with the main project.

if you make a change to the project and want it preserved on github:
1. return to the github desktop app
2. (maybe) type a comment in the lower left box, and commit
3. at the top of the screen there will be a push button this will save your changes to the online repository


the pull request is basically about bringing your branch to the main project 
so your changes become part of the main project




## Unity Instructions:

### to begin:


1. open unityHub
2. click Add and select the FarmFightUnity folder from the github repository
3. if it asks you about the editor, we dont care
3. click and wait for it to open
4. now you wont have to redo it as long as you dont change where the repository is saved
5. be sure to push after saving so you dont lose progress



## Multiplayer Instructions:

By default the game starts as host, so you can't test out being just a client. To try out multiplayer for yourself:

1. Go to the MultiplayerWorldManager game object in the scene and uncheck "Start as Host".
2. Start the game to test. You should see three buttons in the top left. Nothing will work unless you click the "Host" button.
3. Stop the game and in the top menu open ParrelSync -> Clones Manager.
4. Click "Add new clone". ParrelSync will start copying files. This might take a while.
5. You should see "Clone 0" appears at the top of the ParrelSync window. Click "Open in New Editor" to start a new instance of Unity. This also might take a while.
6. After Unity has set up the new editor, start the game on both editor instances.
7. On your *main* instance click "Host" in the upper left, and on your *clone* instance click "Client"
8. If your screen is big enough, resize your editors so each takes up only half of the screen.
9. Interact with the game and see what syncs!
