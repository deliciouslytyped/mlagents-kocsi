build from file->buld or whatever and save in project>\builds

Terminal 1:
	cd <path to project>
	mlagents-learn --width=800 --height=600 --resume --run-id=run-84 scr-noclone.yaml --env=builds\kocsi
Terminal 2:
	cd <path to project>
	tensorboard --logdir results
	
// mlagents-learn --time-scale 1 --width=800 --height=600 --initialize-from=run-79 --run-id=run-84 scr-noclone.yaml --env=builds\kocsi
