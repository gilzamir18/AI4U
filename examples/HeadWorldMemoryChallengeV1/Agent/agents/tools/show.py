import csv
import numpy as np
import matplotlib
from matplotlib import pyplot as plt

reward_files1 = ['experimento1/run-test-run_1589751323_unkrev_env_0-tag-rl_episode_reward_sum.csv', 
		'experimento1/run-test-run_1589792993_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento1/run-test-run_1589833414_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento1/run-test-run_1589905834_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento1/run-test-run_1589974706_unkrev_env_0-tag-rl_episode_reward_sum.csv']

reward_files2 = ['experimento2/run-test-run_1590015857_unkrev_env_0-tag-rl_episode_reward_sum.csv', 
		'experimento2/run-test-run_1590055153_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento2/run-test-run_1590098553_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento2/run-test-run_1590139823_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento2/run-test-run_1590183199_unkrev_env_0-tag-rl_episode_reward_sum.csv']

reward_files3 = ['experimento3/run-test-run_1590234696_unkrev_env_0-tag-rl_episode_reward_sum.csv', 
		'experimento3/run-test-run_1590271907_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento3/run-test-run_1590319285_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento3/run-test-run_1590356192_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento3/run-test-run_1590407271_unkrev_env_0-tag-rl_episode_reward_sum.csv']

reward_files4 = ['experimento4/run-test-run_1590448575_unkrev_env_0-tag-rl_episode_reward_sum.csv', 
		'experimento4/run-test-run_1590489647_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento4/run-test-run_1590534488_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento4/run-test-run_1590574408_unkrev_env_0-tag-rl_episode_reward_sum.csv',
		'experimento4/run-test-run_1590622572_unkrev_env_0-tag-rl_episode_reward_sum.csv']

values_files = []

eplength_files = []


entropy_files = []


def myplot(ax, values, xlabel, ylabel,  title, fontsize=12, color="black", label="method1"):
	 line, = ax.plot(values, color=color)
	 line.set_label(label)
	 ax.legend()
	 ax.locator_params(nbins=3)
	 ax.set_xlabel(xlabel, fontsize=fontsize)
	 ax.set_ylabel(ylabel, fontsize=fontsize)
	 ax.set_title(title, fontsize=fontsize)

def get_smoothed_avg(paths, max_time = 36000, sample_interval = 3600, samples = 5):
	reader = []
	files = []
	for file_path in paths:
		fi = open(file_path, 'r')
		files.append(fi)
		reader.append(csv.reader(fi, delimiter=','))

	avg_total = []
	delta_sum = 0
	while delta_sum <= max_time:
		total = np.zeros(len(reader))
		current_time = np.zeros(len(reader))
		first_time = np.zeros(len(reader))
		min_delta = 10000000000000000000000000000000000
		for i in range(len(reader)):
			j = 0
			for row in reader[i]:
				if j  > 0:
					if j == 1:
						first_time[i] = float(row[0])
					current_time[i] = float(row[0])		 	
					total[i] += float(row[2])
					delta = current_time[i] - first_time[i]
					if delta >= sample_interval:
						if min_delta > delta:
							min_delta = delta
						first_time[i] = current_time[i]
						break;
				j += 1
		delta_sum += min_delta
		avg_total.append(np.mean(total))
	print(len(avg_total))
	for i in range(len(reader)):
		files[i].close()
	return avg_total

#plt.rcParams['savefig.facecolor'] = "0.8"
#plt.rcParams['figure.figsize'] = 4.5, 50
#plt.rcParams['figure.constrained_layout.use'] = True



fig = plt.figure()
#fig.subplots_adjust(bottom=-0.5)

ax_rw = plt.subplot2grid((1,1), (0, 0))
#ax_vl = plt.subplot2grid((2,3),(0, 2))
#ax_epl = plt.subplot2grid((2, 3),(1, 1))
#ax_ent = fig.add_subplot(224)



myplot(ax_rw, get_smoothed_avg(reward_files1, max_time=11*3600, sample_interval=3600), "Time (hours)", "Reward Sum", "", fontsize=10, color="red", label="A3CMAS + GPDRL")
myplot(ax_rw, get_smoothed_avg(reward_files2, max_time=11*3600, samples=4, sample_interval=3600), "Time (hours)", "Reward Sum", "", fontsize=10, color="green", label="A3CMAS Default")
myplot(ax_rw, get_smoothed_avg(reward_files3, max_time=11*3600, samples=4, sample_interval=3600), "Time (hours)", "Reward Sum", "", fontsize=10, color="blue", label="A3CMAS + Entorpy")
myplot(ax_rw, get_smoothed_avg(reward_files4, max_time=11*3600, samples=4, sample_interval=3600), "Time (hours)", "Reward Sum", "", fontsize=10, color="#00ffBB", label="A3CMAS + Placebo")

#myplot(ax_vl, get_smoothed_avg(values_files), "(b) Episode", "Episode Value", "", fontsize=10, color="green")
#myplot(ax_epl, get_smoothed_avg(eplength_files), "(c) Episode", "Episode Length", "", fontsize=10, color="blue")
#myplot(ax_ent, get_smoothed_avg(entropy_files), "(d) Episode", "Average Entropy", "", color="red")

plt.tight_layout()
plt.show()


