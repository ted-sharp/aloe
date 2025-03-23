# Aloe Medock Resvation Library

オニオンアーキテクチャの中心よりの部分を管理している。

DB に関しては、EFCore を使用して SQL を書かないことで移植性を維持している。

なお、DateOnly 型をマッピングするために DBMS 依存の型名を使用している箇所がある。
移植性を維持するなら、DateOnly 型は使わず、従来通り DateTime 型を使う必要がある。
