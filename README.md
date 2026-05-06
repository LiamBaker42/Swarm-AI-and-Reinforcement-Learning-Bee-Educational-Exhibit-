README: Honeybee Communication Exhibit
This project is an interactive, bio-inspired 3D simulation designed as an educational exhibit to demonstrate the complexities of honeybee communication and navigation. It utilizes a hybrid AI framework that combines rule-based Swarm Intelligence (Artificial Bee Colony algorithm) with Reinforcement Learning (Unity ML-Agents).

How It Works
The simulation models the biological honeybee waggle dance as a vector-based communication channel.

Swarm Intelligence (ABC Algorithm): Manages high-level colony roles. Bees transition between Scouts (exploring for new nectar), Workers (harvesting known sources), and Onlookers (waiting at the hive to interpret dances).

Reinforcement Learning (PPO): Individual agents are trained using Proximal Policy Optimization to interpret solar-relative angles and distances communicated via the waggle dance.

Sun-Compass Navigation: The system models solar movement. Agents use the sun's current position as a constant navigational reference, adjusting their flight paths based on the time of day.

Interactive Exhibit: Users can manipulate the environment in real-time by shuffling nectar locations or adding obstacles to observe how the AI agents adapt their pathfinding dynamically.

How to Run
Prerequisites
Unity: Version 6000.0.57f (developed on this version).

Python: Version 3.10.12.

ML-Agents Toolkit: Version 4.0.

Anaconda: Recommended for environment management.

Setup and Installation
Clone the Repository

Unity Setup:

Open the project folder in Unity Hub.

Ensure the ML-Agents Unity Package is correctly installed via the Package Manager.

Python Environment Setup:

Open your terminal (or Anaconda Prompt) and create a new environment:

Bash
conda create -n honeybee-ai python=3.10.12
conda activate honeybee-ai
pip install mlagents==0.40.0 
Running the Simulation
Play Mode: Simply press the Play button in the Unity Editor to run the pre-trained simulation.

Training (Optional):

To start a new training session, run the following command in your Python terminal:

Bash
mlagents-learn config/trainer_config.yaml --run-id=HoneybeeTrain_01
Press Play in Unity to begin the training cycles.

Interacting:

Use the UI Panels to monitor colony efficiency and neural network observations.

Click on individual bees to lock the camera and view their "mental target" and sensory data.

Use the Interactive Buttons at the bottom of the screen to relocate nectar and test agent adaptability.

Key Performance Metrics
Stable Performance: Optimized to run at 60 FPS or higher.

Educational Impact: Rated 4.3/5.0 in user evaluations for effectively communicating biological and computational concepts.
