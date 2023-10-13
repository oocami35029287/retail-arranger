# retail-arranger

The "retail arranger" can be used in conjunction with the "socially-store-robot." This application has the following features:
1. Quickly open ROS project.
2. List the ROS commands you want to open in a selectable format.
3. Visual editing of [Pedsim](https://github.com/srl-freiburg/pedsim_ros).
4. Quickly select the desired test scene to open.



## Dependacy Installation

Needed Project:
   - [socially-store-robot
](https://github.com/oocami35029287/socially-store-robot)
    
Installation
```bash
#used for multi-terminal function
sudo apt-get install tmux
#used for arrange app windows
#install it in your ***docker***
sudo apt-get install wmctrl

```
## Usage introduction


### 1. Configure and preparation
1. Once you install this program, you will use these few files.
    - app: `/run/run.x86_64`
    - config: `/run/run_Data/config/config.yaml`
    - log: `/run/run_Data/config/debug_log.txt`

3.  Please ensure that the path in `config` is directed to your program's directory. 
     - `FileDir`: path of your docker shell script.
     - `mapsDir`: path of amcl global map.
     - `sceneDir`: path of pedsim scenarial file(.xml).
4. Make sure your Docker image name, Docker run, and Docker exec commands are correct. If you are running your program locally, set `Enabled` to false.
5. If you want to add executable programs, please add them to `LaunchItems` in the LaunchItems section. Please make sure the names are not duplicated.
6. If your program opens windows, you can add your window name in `AlignWindows` to position and size the window as desired.

### 2. Application Functions

now you can open the application [here](https://github.com/oocami35029287/retail-arranger/blob/master/run/run.x86_64).
And next, we will introduce the functionality.

![image](https://github.com/oocami35029287/retail-arranger/blob/master/doc/retail%20arranger%20demo.png)

1. ROS Launcher
    - `start`: Just start all launchers you checked.
    - `close`: Close all ros process.
    - `stop`(red button): Force close the docker.
    - `launcher`(the checkboxes) : Choose your process to launch.
2. Selector
    - `Map`(map selector): Select the map you want to open. 
        - You can add your own `.jpg` map in to `/run/run_Data/config/maps/`. 
        - The `.jpg` file name will be use in `config.yaml`as`{curMapItem}` .
    - `Scene Arrangement`: select the Scene
        - You can open your arranged scenarios here, the scenario and be edit by scenario editor.

3. Scenario editor
    - `robot`: Change your robot position and angular.
    - `pedestrian`: Add pedestrians.The number means number of human in a group.
    - `waypoint`:waypoint the waypoint of pedestrian and robot.(the waypoint of robot remain unsolved to functionality.)
    - `wall`: In `pedsim` humans cannot aware the world surrondings in `gazebo`, so you need to add obstacles manually.
    - `select`: Select your target pedestrian or robot to add waypoint.
    - `eraser`: Just earse the icons on map.
    - `broom`: Clear the whole map.
    - `save`: Save this scenario and you can load it in `Scene Arranger`.
