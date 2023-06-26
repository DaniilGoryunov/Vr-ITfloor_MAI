using UnityEngine;
using System.Collections;

namespace RoomArchitectEngine
{
    /// <summary>
    /// This object represents a single wall block (Refer to the documentation for more)
    /// </summary>
    public class WallPart
    {
        public Position position;
        public Mod[] mods;
        public string[] seperationID;
        public bool hideBuffers = false;
        public BoxTexture[] textures;
        public bool[] secondUse = { false, false, false, false, false, false, false, false, false };

        public WallPart(Position position, float thickness)
        {
            mods = new Mod[7] { Mod.NONE, Mod.NONE, Mod.NONE, Mod.NONE, Mod.NONE, Mod.NONE, Mod.NONE };
            seperationID = new string[7] { "", "", "", "", "", "", "" };
            textures = new BoxTexture[7];
            for (int i = 0; i < textures.Length; i++)
                textures[i] = new BoxTexture();
            mods[Directions.CENTER] = Mod.FULL;
            this.position = position;
        }

        public bool onlyCenterIsActive()
        {
            if (mods[Directions.EAST] != Mod.NONE ||
                mods[Directions.WEST] != Mod.NONE ||
                mods[Directions.NORTH] != Mod.NONE ||
                mods[Directions.SOUTH] != Mod.NONE)
                return false;
            return true;
        }

        public void makeArchThroughX()
        {
            mods[Directions.CENTER].Set(Mod.ONLYTOP);
            mods[Directions.EAST].Set(Mod.ONLYTOP);
            mods[Directions.WEST].Set(Mod.ONLYTOP);
            mods[Directions.NORTH].Set(Mod.NONE);
            mods[Directions.SOUTH].Set(Mod.NONE);
            hideBuffers = true;
        }

        public void makeArchThroughZ()
        {
            mods[Directions.CENTER].Set(Mod.ONLYTOP);
            mods[Directions.NORTH].Set(Mod.ONLYTOP);
            mods[Directions.SOUTH].Set(Mod.ONLYTOP);
            mods[Directions.EAST].Set(Mod.NONE);
            mods[Directions.WEST].Set(Mod.NONE);
            hideBuffers = true;
        }

        public void setMod(Directions dir, Mod mod, string sepID, BoxTexture textureBox, bool otherSide, bool cancelTexturing = false, Room owner = null)
        {
            mods[dir].Set(mod);
            bool isVertical = true;
            if (dir == Directions.WEST || dir == Directions.EAST)
                isVertical = false;
            if (owner != null)
                mods[dir].addOwner(owner);
            if (sepID != "_IGNORE_")
                this.seperationID[dir] = sepID;
            if (cancelTexturing)
                return;
            setTextures(textureBox, dir, isVertical, otherSide);
        }

        public void setSep(Directions dir, string separationID)
        {
            this.seperationID[dir] = separationID;
        }

        public void setTextures(BoxTexture tex, Directions dir, bool vertical, bool inverted)
        {
            if (secondUse[dir] == false)
            {
                textures[dir] = tex.copy();
                //textures[Directions.CENTER] = tex.copy();
                if (dir.isEither(Directions.NORTH, Directions.SOUTH))
                {
                    textures[Directions.CENTER].textureID[Directions.WEST] = tex.textureID[Directions.WEST];
                    textures[Directions.CENTER].textureID[Directions.EAST] = tex.textureID[Directions.EAST];
                }
                if (dir.isEither(Directions.WEST, Directions.EAST))
                {
                    textures[Directions.CENTER].textureID[Directions.NORTH] = tex.textureID[Directions.NORTH];
                    textures[Directions.CENTER].textureID[Directions.SOUTH] = tex.textureID[Directions.SOUTH];
                }
                secondUse[dir] = true;
            }
            else
            {
                if (vertical)
                {
                    if (inverted)
                    {
                        textures[dir].textureID[Directions.WEST] = tex.textureID[Directions.WEST];
                        textures[Directions.CENTER].textureID[Directions.WEST] = tex.textureID[Directions.WEST];
                    }
                    else
                    {
                        textures[dir].textureID[Directions.EAST] = tex.textureID[Directions.EAST];
                        textures[Directions.CENTER].textureID[Directions.EAST] = tex.textureID[Directions.EAST];
                    }
                }
                else
                {
                    if (inverted)
                    {
                        textures[dir].textureID[Directions.SOUTH] = tex.textureID[Directions.SOUTH];
                        textures[Directions.CENTER].textureID[Directions.SOUTH] = tex.textureID[Directions.SOUTH];
                    }
                    else
                    {
                        textures[dir].textureID[Directions.NORTH] = tex.textureID[Directions.NORTH];
                        textures[Directions.CENTER].textureID[Directions.NORTH] = tex.textureID[Directions.NORTH];
                    }
                }
            }
        }
    }
}
