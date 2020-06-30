from matplotlib import pyplot as plt
import csv
import argparse
import numpy as np
import sys

log_reg_name = "logs/%s/log_%d_%d"

color_map = {8080:['#ff0000', '#00ffff'], 8081:['#00ff00','#ff00ff'], 8082: ['#0000ff','#ffff00'],
				8083:['#990000','#009999'], 8084:['#009900', '#990099'], 8085:['#000099', '#999900'],
				8086:['#330000', '#003333'], 8087:['#003300', '#330033']}

class Agent:
	def __init__(self, name, x=None, y=None, z=None, state=None):
		self.name = name
		if x is None:
			self.x_trail = []
		else:
			self.x_trail = x
		
		if y is None:
			self.y_trail = []
		else:
			self.y_trail = y

		if z is None:
			self.z_trail = []
		else:
			self.z_trail = z
		
		if state is None:
			self.state = []
		else:
			self.state = state

class Graphics:
	def __init__(self):
		self.fig = plt.figure()
		self.ax = self.fig.add_subplot(111, projection = '3d')
		self.ax.set_title("Interaction Map", fontsize=12)
	def plot(self, agent):
		for i in range(0, len(agent.x_trail), 20):
			self.ax.scatter(agent.x_trail[i], agent.y_trail[i], agent.z_trail[i], color=agent.state[i]['color'])
		self.ax.plot(agent.x_trail, agent.y_trail, agent.z_trail, color=color_map[agent.name][0])
		self.ax.locator_params(nbins=3)
		fontsize = 10
		self.ax.set_xlabel("Terrain X Axis", fontsize=fontsize)
		self.ax.set_ylabel("Terrain Z Axis", fontsize=fontsize)
		self.ax.set_zlabel("Simulation Time", fontsize=fontsize)


	def show(self):
		plt.show()

def load_agent(dir, id, ep):
	path = log_reg_name%(dir, id, ep)
	with  open(path, 'r') as csvfile:
		logreader = csv.reader(csvfile, delimiter=',')
		xs = []
		ys = []
		zs = []
		state = []
		for row in logreader:
			ocolor = "#ff0000"
			ccolor = "#000000"
			istate = float(row[5])
			ostate = float(row[6])
			xs.append(float(row[2]))
			ys.append(float(row[4]))
			zs.append(float(row[0]))
			if istate == 1:
				state.append({"color":ocolor, 'value': 1, 'other': ostate})
			else:
				state.append({"color":ccolor, 'value': 0, 'other': ostate})
	return Agent(id, xs, ys, zs, state)

def get_reward_avg(agent):
	total = 0
	sum = 0
	for state  in agent.state:
		if  state['other'] >= 0:
			if state['other'] == 0 and state['value'] == 0:
				sum += -5
			elif state['other'] == 0 and state['value'] == 1:
				sum += -10
			elif state['other'] == 1 and state['value'] == 0:
				sum += 10
			elif state['other'] == 1 and state['value'] == 1:
				sum += 20
			total += 1

	return sum/total if total > 0 else 0

def get_agents_info(dir='test1_log_ex1', episode=1):
	g = Graphics()
	agent_info = []
	total = 0
	for id in [8080, 8081, 8082, 8083, 8084,8085, 8086, 8087]:
		agent = load_agent(dir, id, episode)
		score = get_reward_avg(agent)
		total += score
		print(score)
		agent_info.append(score)
		g.plot(agent)
	g.show()
	return agent_info, total

def get_agents_totalscore(path='test1_log_ex1'):
	total = {8080:0, 8081:0, 8082:0, 8083:0, 8084:0,8085:0, 8086:0, 8087:0}
	IDs = [8080, 8081, 8082, 8083, 8084,8085, 8086, 8087]
	for i in range(0, 10):
		for id in IDs:
			agent = load_agent(path, id, i)
			total[id] += get_reward_avg(agent)
	score = 0
	score_by_agent = []
	for id in IDs:
		m = total[id]/10
		print("Agent ", id, " Score: ", m)
		score += m
		score_by_agent.append(m)
	print("Total Score: ", score)
	return score, score_by_agent

def get_experiments_score(experiments=None):
	if experiments == None:
		experiments = ['test1_log_ex1', 'test2_log_ex1', 'test3_log_ex1', 'test4_log_ex1', 'test5_log_ex1']
	
	agents_avgs = [0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0]
	total_avg = 0.0
	score_by_model = np.zeros(len(experiments))
	for idx,ex in enumerate(experiments):
		score, score_by_agent = get_agents_totalscore(ex)
		score_by_model[idx] = np.sum(score_by_agent)
		for i in range(len(agents_avgs)):
			agents_avgs[i] += score_by_agent[i]
		total_avg += score
	avg = total_avg/len(experiments)
	total_sd = 0.0
	for ex in experiments:
		score, score_by_agent = get_agents_totalscore(ex)
		total_sd = (score - avg) ** 2
	return avg, np.array(agents_avgs)/len(experiments), np.sqrt(total_sd)/len(experiments), score_by_model



def parse_args():
	parser = argparse.ArgumentParser()
	parser.add_argument("--run",
						choices=['episode', 'test'],
						default='episode')
	parser.add_argument("--path", default='test1_log_ex1')
	parser.add_argument('--ep', type=int, default=0)
	parser.add_argument("--start", type=int, default=1)
	parser.add_argument("--stop", type=int, default=5)
	parser.add_argument('--ex', type=int, default=1)
	return parser.parse_args()

if __name__=='__main__':
	args = parse_args()
	if args.run == "episode":
		print(get_agents_info(args.path, args.ep))
	elif args.run == "test":
		experiments = []
		for i in range(args.start, args.stop+1):
			experiments.append("test%d_log_ex%d"%(i, args.ex))
		score_avg, agent_score_avg, sd, score_by_model = get_experiments_score(experiments)
		print("Score by agent: ", np.array(agent_score_avg))
		print("Total Mean Score: ", score_avg)
		print("SD: ", sd)
		print("Score by model, ", score_by_model)
	else:
		print('Invalid option : ', args.run)


