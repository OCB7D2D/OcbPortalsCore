﻿using System.IO;

public interface IfacePersistable
{

    string GetBackupPath();
    string GetStoragePath();
    string GetThreadKey();

    // Implement your write functionality
    void Write(BinaryWriter bw);

    // Implement your read functionality
    void Read(BinaryReader br);


}
