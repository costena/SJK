# SJK
SJK是一个好用的数据库对象映射库，可以将CURD转换为对象的操作。使用该库无需编写SQL。
例如：
假设数据库有一个表名为 Users，其有两个字段userID(int)和userName(string)。现在，插入数据可以这样做：
首先建立一个对象如下：

\[Table("Users")]\n
class Users {\n
  \[PrimaryKey("userID")]\n
  int userID;\n
  \[Key("userName")]\n
  string userName;\n
 }\n
 \n
 然后，执行\n
  Session.Insert(new Users());\n
 即可。\n
 更多用法以后再说。\n
