using System;
using Microsoft.Xna.Framework;
using MonoGameJam1.Components.Map;
using MonoGameJam1.Scenes;
using Nez;
using System.Collections.Generic;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Battle.Enemies;
using Random = Nez.Random;

namespace MonoGameJam1.Systems
{
    public class BattleAreasSystem : EntityProcessingSystem
    {
        private readonly Entity _blockEntity;
        private readonly Entity _playerEntity;

        // Battle
        private bool _battleHappening;
        private BattleAreaComponent _currentBattle;
        private float _spawnInterval;
        private int _currentWave;
        private int _enemiesSpawned;
        private int _enemiesDefeated;
        private List<Entity> _enemies;
        private List<Entity> _enemiesToRemove;

        public BattleAreasSystem(Entity playerEntity) : base(new Matcher().one(typeof(BattleAreaComponent)))
        {
            _playerEntity = playerEntity;

            _blockEntity = playerEntity.scene.createEntity();
            _blockEntity.addComponent(new BoxCollider(0, 0, 16, Scene.virtualSize.Y));
            _blockEntity.enabled = false;

            _enemies = new List<Entity>();
            _enemiesToRemove = new List<Entity>();
        }

        protected override void process(List<Entity> entities)
        {
            base.process(entities);

            if (_battleHappening)
            {
                updateBattle();
            }
        }

        private void updateBattle()
        {
            if (_spawnInterval > 0.0f)
            {
                _spawnInterval -= Time.deltaTime;
                return;
            }
            
            foreach (var enemy in _enemies)
            {
                if (enemy.getComponent<BattleComponent>().Dying)
                {
                    _enemiesToRemove.Add(enemy);
                    _enemiesDefeated++;
                }
            }
            _enemiesToRemove.ForEach(x => _enemies.Remove(x));
            _enemiesToRemove.Clear();

            if (_currentWave >= _currentBattle.Waves.Length)
            {
                Console.WriteLine("finished!");
                resetBattle();
                return;
            }

            if (_enemiesDefeated >= _currentBattle.Waves[_currentWave])
            {
                _currentWave++;
                _enemiesDefeated = 0;
                _enemiesSpawned = 0;
                _spawnInterval = 0.5f;
            }

            if (_spawnInterval <= 0.0f && _enemiesSpawned < _currentBattle.Waves[_currentWave])
            {
                var playerScene = _playerEntity.scene as SceneMap;
                var enemyName = _currentBattle.Enemies.randomItem();
                var areaCollider = _currentBattle.collider;
                var areaBounds = areaCollider.bounds;

                // create enemy entity
                if (playerScene != null)
                {
                    EnemyComponent enemyComponent;
                    var enemy = playerScene.createEnemy(enemyName, false, out enemyComponent);
                    var widthOffset = areaBounds.width * 0.1f;
                    var positionX = areaBounds.x + widthOffset + Random.nextFloat(areaBounds.width - widthOffset * 2);
                    enemy.setPosition(positionX, areaCollider.localOffset.Y + areaCollider.height);

                    var spawnable = enemyComponent as ISpawnableEnemy;
                    if (spawnable != null)
                    {
                        spawnable.GoToSpawnState();
                        spawnable.MovableArea = areaBounds;
                    }

                    _enemies.Add(enemy);
                }

                _spawnInterval = 0.4f;
                _enemiesSpawned++;
            }
        }

        public override void process(Entity entity)
        {
            if (_battleHappening) return;

            var battleArea = entity.getComponent<BattleAreaComponent>();
            if (battleArea.Activated) return;

            var collider = entity.getComponent<BoxCollider>();

            var collisionRect = Physics.overlapRectangle(collider.bounds, 1 << SceneMap.PLAYER_LAYER);
            if (collisionRect != null)
            {
                startBattle(battleArea);
                _blockEntity.enabled = true;
                _blockEntity.setPosition(new Vector2(collider.absolutePosition.X + collider.width / 2, collider.bounds.y));
            }
        }

        protected override void lateProcess(List<Entity> entities)
        {
            base.lateProcess(entities);

            // Check if the player is colliding with the block entity
            if (!_battleHappening) return;

            CollisionResult collisionResult;
            if (_blockEntity.getComponent<BoxCollider>()
                .collidesWith(_playerEntity.getComponent<BoxCollider>(), out collisionResult))
            {
                _playerEntity.transform.position += collisionResult.minimumTranslationVector;
            }
        }

        private void startBattle(BattleAreaComponent battleAreaComponent)
        {
            resetBattle();
            _battleHappening = true;
            _currentBattle = battleAreaComponent;
            _currentBattle.SetActivated();
        }

        private void resetBattle()
        {
            _battleHappening = false;
            _currentWave = 0;
            _spawnInterval = 0;
        }
    }
}
