﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameJam1.Components;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Map;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Components.Windows;
using MonoGameJam1.Managers;
using MonoGameJam1.NPCs;
using MonoGameJam1.Scenes.SceneMapExtensions;
using MonoGameJam1.Structs;
using MonoGameJam1.Systems;
using Nez;
using Nez.Sprites;
using Nez.Tiled;

namespace MonoGameJam1.Scenes
{
    class SceneMap : Scene
    {
        //--------------------------------------------------
        // Render Layers Constants

        public const int BACKGROUND_RENDER_LAYER = 10;
        public const int TILED_MAP_RENDER_LAYER = 9;
        public const int WATER_RENDER_LAYER = 6;
        public const int MISC_RENDER_LAYER = 5;
        public const int ENEMIES_RENDER_LAYER = 4;
        public const int PLAYER_RENDER_LAYER = 3;
        public const int PARTICLES_RENDER_LAYER = 2;
        public const int HUD_BACK_RENDER_LAYER = 1;
        public const int HUD_FILL_RENDER_LAYER = 0;

        //--------------------------------------------------
        // Layer Masks

        public const int MAP_LAYER = 0;
        public const int PLAYER_LAYER = 1;
        public const int ENEMY_LAYER = 2;
        public const int PROJECTILES_LAYER = 3;

        //--------------------------------------------------
        // Tags

        public const int PROJECTILES = 1;
        public const int TRAPS = 3;

        //--------------------------------------------------
        // Map

        private TiledMapComponent _tiledMapComponent;
        private TiledMap _tiledMap;

        //--------------------------------------------------
        // Camera

        private CameraSystem _camera;

        //--------------------------------------------------
        // Map Extensions

        private List<ISceneMapExtensionable> _mapExtensions;

        //--------------------------------------------------
        // Paths

        private MapPath[] _paths;

        //--------------------------------------------------
        // Battle Areas
        
        private Entity _currentBattleArea;

        //--------------------------------------------------
        // Player

        private PlayerComponent _playerComponent;

        //--------------------------------------------------
        // HUD

        private Entity _hudEntity;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(54, 72, 130);
            setupMap();
            setupBattleAreas();
            setupPlayer();
            setupPaths();
            setupEnemies();
            setupNpcs();
            setupHud();
            setupMapExtensions();
            setupPostProcessors();
            setupTransfers();
        }

        public override void onStart()
        {
            setupEntityProcessors();
            getEntityProcessor<NpcInteractionSystem>().mapStart();
            Core.getGlobalManager<InputManager>().IsLocked = false;
        }

        private void setupMap()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            var mapId = sysManager.MapId;
            _tiledMap = content.Load<TiledMap>($"maps/map{mapId}");
            sysManager.setTiledMapComponent(_tiledMap);

            var tiledEntity = createEntity("tiledMap");
            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var defaultLayers = _tiledMap.properties["defaultLayers"].Split(',').Select(s => s.Trim()).ToArray();

            _tiledMapComponent = tiledEntity.addComponent(new TiledMapComponent(_tiledMap, collisionLayer) { renderLayer = TILED_MAP_RENDER_LAYER });
            _tiledMapComponent.setLayersToRender(defaultLayers);

            if (_tiledMap.properties.ContainsKey("aboveWaterLayer"))
            {
                var aboveWaterLayer = _tiledMap.properties["aboveWaterLayer"];
                var tiledAboveWater = tiledEntity.addComponent(new TiledMapComponent(_tiledMap) { renderLayer = WATER_RENDER_LAYER });
                tiledAboveWater.setLayerToRender(aboveWaterLayer);
            }
        }

        private void setupBattleAreas()
        {
            var battleAreasGroup = _tiledMap.getObjectGroup("battleAreas");
            if (battleAreasGroup == null) return;

            var battleAreas = battleAreasGroup.objects;

            foreach (var battleArea in battleAreas)
            {
                var battleAreaComponent = new BattleAreaComponent();
                if (battleArea.properties.ContainsKey("enemies"))
                {
                    var enemies = battleArea.properties["enemies"].Split(',');
                    battleAreaComponent.Enemies = enemies;
                }
                if (battleArea.properties.ContainsKey("waves"))
                {
                    var waves = battleArea.properties["waves"].Split(',').Select(int.Parse).ToArray();
                    battleAreaComponent.Waves = waves;
                }

                var entity = createEntity();
                entity
                    .addComponent(new BoxCollider(0, 0, battleArea.width, battleArea.height))
                    .addComponent(battleAreaComponent);
                entity.setPosition(battleArea.position);
            }
        }

        private void setupPlayer()
        {
            var systemManager = Core.getGlobalManager<SystemManager>();

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            Vector2? playerSpawn;

            if (systemManager.SpawnPosition.HasValue)
            {
                playerSpawn = systemManager.SpawnPosition;
            }
            else
            {
                playerSpawn = _tiledMap.getObjectGroup("objects").objectWithName("playerSpawn").position;
            }

            var player = createEntity("player");
            player.transform.position = playerSpawn.Value;
            player.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            var collider = player.addComponent(new BoxCollider(-8f, -19f, 16f, 42f));
            Flags.setFlagExclusive(ref collider.physicsLayer, PLAYER_LAYER);

            player.addComponent(new InteractionCollider(-20f, -6, 40, 22));
            player.addComponent(new BattleComponent());
            player.addComponent(new PlatformerObject(_tiledMap));
            player.addComponent<TextWindowComponent>();

            // Player Component

            var playerComponent = player.addComponent<PlayerComponent>();
            playerComponent.sprite.renderLayer = PLAYER_RENDER_LAYER;

            // Set player
            _playerComponent = playerComponent;
            systemManager.setPlayer(player);
        }
        
        private void setupPaths()
        {
            var pathObjectGroup = _tiledMap.getObjectGroup("paths");
            if (pathObjectGroup == null) return;

            var objects = pathObjectGroup.objects;

            _paths = new MapPath[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                _paths[i] = new MapPath
                {
                    Name = objects[i].name,
                    Start = objects[i].position,
                    End = objects[i].position + new Vector2(objects[i].width, objects[i].height)
                };
            }
        }

        private void setupEnemies()
        {
            var enemiesGroup = _tiledMap.getObjectGroup("enemies");
            if (enemiesGroup == null) return;
            foreach (var enemy in enemiesGroup.objects)
            {
                var patrolStartRight = bool.Parse(enemy.properties.ContainsKey("patrolStartRight")
                    ? enemy.properties["patrolStartRight"]
                    : "false");

                EnemyComponent enemyComponent;
                var entity = createEnemy(enemy.type, patrolStartRight, out enemyComponent);
                if (enemy.properties.ContainsKey("path"))
                {
                    var pathName = enemy.properties["path"];
                    var path = _paths.First(x => x.Name == pathName);
                    enemyComponent.path = path;
                }
                entity.transform.position = enemy.position + new Vector2(enemy.width, enemy.height) / 2;
            }
        }

        public Entity createEnemy(string enemyName, bool patrolStartRight, out EnemyComponent enemyComponent)
        {
            var collisionLayer = _tiledMap.properties["collisionLayer"];

            var entity = createEntity("enemy");
            entity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            entity.addComponent(new PlatformerObject(_tiledMap));
            entity.addComponent<BattleComponent>();
            var collider = entity.addComponent(new BoxCollider(-22f, -9f, 44f, 32f));
            Flags.setFlagExclusive(ref collider.physicsLayer, ENEMY_LAYER);

            var instance = createEnemyInstance(enemyName, patrolStartRight);
            enemyComponent = entity.addComponent(instance);
            enemyComponent.sprite.renderLayer = ENEMIES_RENDER_LAYER;
            enemyComponent.playerCollider = findEntity("player").getComponent<BoxCollider>();
            
            return entity;
        }

        private EnemyComponent createEnemyInstance(string enemyName, bool patrolStartRight)
        {
            var enemiesNamespace = typeof(BattleComponent).Namespace + ".Enemies";
            var type = Type.GetType(enemiesNamespace + "." + enemyName + "Component");
            if (type != null)
            {
                return Activator.CreateInstance(type, new object[] { patrolStartRight }) as EnemyComponent;
            }
            return null;
        }

        private void setupEntityProcessors()
        {
            var player = findEntity("player");
            var playerComponent = player.getComponent<PlayerComponent>();
            
            var mapSize = new Vector2(_tiledMap.widthInPixels, _tiledMap.heightInPixels);
            _camera = new CameraSystem(player)
            {
                mapLockEnabled = true,
                mapSize = mapSize,
                followLerp = 0.08f,
                deadzoneSize = new Vector2(20, 10)
            };
            addEntityProcessor(_camera);

            var battleSystem = (BattleSystem)addEntityProcessor(new BattleSystem(playerComponent));
            battleSystem.BulletEffectTexture = content.Load<Texture2D>(Content.Effects.bullet_effects);
            addEntityProcessor(new ProjectilesSystem(player));
            addEntityProcessor(new BattleAreasSystem(player));

            addEntityProcessor(new TransferSystem(new Matcher().all(typeof(TransferComponent)), player));
            addEntityProcessor(new NpcInteractionSystem(playerComponent));
        }

        private void setupNpcs()
        {
            var npcObjects = _tiledMap.getObjectGroup("npcs");
            if (npcObjects == null) return;

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var names = new Dictionary<string, int>();
            foreach (var npc in npcObjects.objects)
            {
                names[npc.name] = names.ContainsKey(npc.name) ? ++names[npc.name] : 0;

                var npcEntity = createEntity($"{npc.name}:{names[npc.name]}");
                var npcComponent = (NpcBase)Activator.CreateInstance(Type.GetType("TimePrototype.NPCs." + npc.type), npc.name);
                npcComponent.setRenderLayer(MISC_RENDER_LAYER);
                npcComponent.ObjectRect = new Rectangle(0, 0, npc.width, npc.height);
                npcEntity.addComponent(npcComponent);
                npcEntity.addComponent<TextWindowComponent>();
                npcEntity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
                npcEntity.position = npc.position;

                if (!npcComponent.Invisible)
                {
                    npcEntity.addComponent(new PlatformerObject(_tiledMap));
                }

                // Props
                if (npc.properties.ContainsKey("flipX") && npc.properties["flipX"] == "true")
                {
                    npcComponent.FlipX = true;
                }
                if (npc.properties.ContainsKey("autorun") && npc.properties["autorun"] == "true")
                {
                    getEntityProcessor<NpcInteractionSystem>().addAutorun(npcComponent);
                }
            }
        }

        private void setupHud()
        {
            _hudEntity = createEntity("hud");
            _hudEntity.addComponent<MapHudComponent>().renderLayer = HUD_BACK_RENDER_LAYER;
        }

        private void setupMapExtensions()
        {
            _mapExtensions = new List<ISceneMapExtensionable>();

            if (!_tiledMap.properties.ContainsKey("mapExtensions")) return;

            var extensions = _tiledMap.properties["mapExtensions"].Split(',').Select(s => s.Trim()).ToArray();

            foreach (var extension in extensions)
            {
                var extensionInstance = (ISceneMapExtensionable)Activator.CreateInstance(Type.GetType("TimePrototype.Scenes.SceneMapExtensions." + extension));
                extensionInstance.Scene = this;
                extensionInstance.initialize();
                _mapExtensions.Add(extensionInstance);
            }
        }

        private void setupPostProcessors()
        {
            /*
            var bloom = addPostProcessor(new BloomPostProcessor(2));
            bloom.setBloomSettings(new BloomSettings(0.5f, 0.4f, 0.6f, 1, 1, 1));

            var scanlines = addPostProcessor(new ScanlinesPostProcessor(1));
            scanlines.effect.attenuation = 0.04f;
            scanlines.effect.linesFactor = 1500f;
            */
        }

        private void setupTransfers()
        {
            var transfers = _tiledMap.getObjectGroup("transfers");
            if (transfers == null) return;

            var names = new Dictionary<string, int>();
            foreach (var transferObj in transfers.objects)
            {
                names[transferObj.name] = names.ContainsKey(transferObj.name) ? ++names[transferObj.name] : 0;

                var entity = createEntity(string.Format("{0}:{1}", transferObj.name, names[transferObj.name]));
                entity.addComponent(new TransferComponent(transferObj));
            }
        }

        public void reserveTransfer(TransferComponent transferComponent)
        {
            Core.getGlobalManager<SystemManager>().setMapId(transferComponent.destinyId);
            Core.getGlobalManager<SystemManager>().setSpawnPosition(transferComponent.destinyPosition);
            Core.startSceneTransition(new FadeTransition(() => new SceneMap()));
        }

        public void reserveTransfer(int mapId, int mapX, int mapY)
        {
            var spawnPosition = new Vector2(mapX, mapY);
            Core.getGlobalManager<SystemManager>().setMapId(mapId);
            Core.getGlobalManager<SystemManager>().setSpawnPosition(spawnPosition);
            Core.startSceneTransition(new FadeTransition(() => new SceneMap()));
        }

        public void sendMessageToExtensions(string message)
        {
            foreach (var extension in _mapExtensions)
            {
                extension.receiveSceneMessage(message);
            }
        }

        public override void update()
        {
            base.update();

            updateHud();

            // Update extensions
            _mapExtensions.ForEach(extension => extension.update());
        }

        private void updateHud()
        {
            var camerapos = _camera.camera.position - virtualSize.ToVector2() / 2;
            _hudEntity.position = new Vector2((int)camerapos.X, (int)camerapos.Y);
        }
    }
}
