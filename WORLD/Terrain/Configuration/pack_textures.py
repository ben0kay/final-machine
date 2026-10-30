# Explicit source-map channel packing; Pillow required. Run from repository root.
from pathlib import Path
r = Path(__file__).resolve().parents[3]
from PIL import Image
for stem,folder in [('grass_ground','GrassGround'),('forest_ground_06','ForestGround06'),('gray_rocks','GrayRocks')]:
 source=r/f'ASSETS/Terrain/{stem}_1k.blend/textures'
 albedo=Image.open(source/f'{stem}_diff_1k.jpg').convert('RGB')
 height=Image.open(source/f'{stem}_disp_1k.png').convert('L')
 normal=Image.open(r/f'ASSETS/Terrain/Runtime/{folder}/Normal.png').convert('RGB')
 rough=Image.open(r/f'ASSETS/Terrain/Runtime/{folder}/Roughness.png').convert('L')
 out=r/'WORLD/Terrain/Configuration/Textures';out.mkdir(exist_ok=True)
 for name,img,alpha in [('AlbedoHeight',albedo,height),('NormalRoughness',normal,rough)]:
  img.putalpha(alpha);img.save(out/f'{folder}{name}.png')
