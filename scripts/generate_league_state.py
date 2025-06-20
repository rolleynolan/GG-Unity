import json
import random
import os

POSITIONS = ["QB", "RB", "WR", "TE", "OL", "DL", "LB", "CB", "S", "K"]


def generate_player(idx):
    return {
        "name": f"Player {idx}",
        "position": random.choice(POSITIONS),
        "age": random.randint(20, 34),
        "overall": random.randint(60, 99),
        "contract": {"years_left": random.randint(1, 5)},
    }


def load_teams(teams_path):
    with open(teams_path, "r") as f:
        data = json.load(f)
    return data


def build_league_state(teams_data, players_per_team=10):
    teams = []
    player_idx = 1
    for team in teams_data:
        roster = [generate_player(player_idx + i) for i in range(players_per_team)]
        player_idx += players_per_team
        teams.append({
            "name": f"{team['city']} {team['name']}",
            "abbreviation": team["abbreviation"],
            "roster": roster,
        })
    league_state = {
        "week": 1,
        "results_by_week": {},
        "teams": teams,
    }
    return league_state


def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    teams_path = os.path.join(root, "Assets", "Resources", "Teams.json")
    teams_data = load_teams(teams_path)
    league_state = build_league_state(teams_data)
    save_dir = os.path.join(root, "save")
    os.makedirs(save_dir, exist_ok=True)
    out_path = os.path.join(save_dir, "league_state.json")
    with open(out_path, "w") as f:
        json.dump(league_state, f, indent=2)
    print(f"Wrote {out_path}")


if __name__ == "__main__":
    main()
