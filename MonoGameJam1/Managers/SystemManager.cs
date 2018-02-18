using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace MonoGameJam1.Managers
{
    public class SystemManager : IUpdatableManager
    {
        //--------------------------------------------------
        // Switches & Variables

        public Dictionary<string, bool> switches;
        public Dictionary<string, int> variables;

        //--------------------------------------------------
        // Postprocessors
        
        public CinematicLetterboxPostProcessor cinematicLetterboxPostProcessor;

        //--------------------------------------------------
        // Player

        public Entity playerEntity;
        public int PlayerScore { get; set; }

        //--------------------------------------------------
        // Map

        private int _mapId;
        public int MapId => _mapId;

        private TiledMap _tiledMapComponent;
        public TiledMap TiledMap => _tiledMapComponent;

        private Vector2? _spawnPosition;
        public Vector2? SpawnPosition => _spawnPosition;

        //----------------------//------------------------//

        public SystemManager()
        {
            switches = new Dictionary<string, bool>();
            variables = new Dictionary<string, int>();
        }

        public void setPlayer(Entity playerEntity)
        {
            this.playerEntity = playerEntity;
        }

        public void setMapId(int mapId)
        {
            _mapId = mapId;
        }

        public void setTiledMapComponent(TiledMap map)
        {
            _tiledMapComponent = map;
        }

        public void setSpawnPosition(Vector2 position)
        {
            _spawnPosition = position;
        }

        public void setSwitch(string name, bool value)
        {
            switches[name] = value;
        }

        public bool getSwitch(string name)
        {
            return switches.ContainsKey(name) ? switches[name] : false;
        }

        public void setVariable(string name, int value)
        {
            variables[name] = value;
        }

        public int getVariable(string name)
        {
            return variables.ContainsKey(name) ? variables[name] : 0;
        }

        public void update()
        { }
    }
}
