Interlook.Configuration
=================

Library for obtaining configuration data from different sources (currently INI-files and command line options) utilizing `Microsoft.Extensions.Configuration`.

May be combined with other existing extensions like `Microsoft.Extensions.Configuration.NewtonsoftJson`.

### Overview

In the `Microsoft.Extensions.Configuration` world, actual configuration is generally stored as key/value-pairs. The key names are **case insensitive** and may pre prefixed with section names. Thus a key, say `server_name` may occur multiple times, eg. like this:

- `database:server_name`
- `nuget_repo:server_name`

The colon is used as key delimiter. Multiple prefixes are possible (like `nuget_repo:primary:connection:server_name`) but providers of this library don't make directly use of it, although you may define likewise.


### Supported sources
#### INI-Files

The classic [Ini-File format](https://en.wikipedia.org/wiki/INI_file) from old DOS times. There is a standard implementation in `Microsoft.Extensions.Configuration.Ini`, which doesn't completely deliver what I wanted.

###### Values
Values are assigned to keys like this:
```ini
keyname=value that includes everything behind the equals sign
another="double quotes are possible and omitted in the value, if they are the first and last characters of the line"
imagine='same goes for single quotes'
```

Please note that multi-line values are **not supported** by the Ini-file format.

###### Comments
Comments are marked with `#` or `;` at the beginning of the line

```ini
# Everything here is not parsed and just a comment
warning=Comments behind values are not possible, so     #this still belongs to the value

  # but you may indent comments, if you insist
colon="possible, and..."
;colon=seems to be used mostly to comment things out
```


###### Sections
Keys can be grouped into sections (as described above) like this:

```ini
# everything before the first section is not prefixed
global_key=has no prefix

[database]
# the following key name will be database:server_name
server_name=127.0.0.1
user_name=caligula

[nuget]
# you get the idea
server_name=stuff.our-intranet.universe

#NOTE: sections cannot be nested, but again
#      if you insist, you could enforce it
[nuget:primary:connection]
server_name=secure_repo.intranet.universe
# but this is against standard 
# and not guaranteed to work always, everywhere and forever
```

That's everything (actually a bit more) that is supported by Ini-files.

#### Command line arguments

Again, there is a standard implementation in `Microsoft.Extensions.Configuration.CommandLine`, but I wanted slightly more control and the opportunity to group short-switches, as known from the unix format.

Unlike with Ini-Files, you have to define the key-names (including possible [multi-]prefixes) in advance and map them to command line arguments.

Command line arguments can be divided into switches, valued options and values.

###### Switches

A switch is a single argument without a further value, mostly just enabling something (like a boolean value).

```console
myTool --quiet
myTool -q
```

The above may be defined als aliases. For switches you define both key name and value in advance. This key/value pair is added to configuration, as soon as the corresponding switch is provided. Switches are always optional, since an mandatory switch would make no sense; what's the point of an argument, that always is to be specified for a program to work.

###### Valued options

There are arguments, which require an additional value to be provided. Consider this:

```console
myTool --user MartinGalway
myTool -v 5
```

These arguments may be optional or required, just as you define it.

Here, you only define the key name in advance, since the actual value is obtained from command line. Omitting the value will cause an error.

###### Values

Finally, the program may require certain values as input. We all know this:

```console
copy -f source_file destination
```

Their order is fixed, the first argument, that is no switch and no value-option is interpreted as first value. For this you define a list of configuration key names, which determines the number and order of the required values.

Of course, all of the three types above can be mixed in any way.

```console
myTool -q first_value --user Armakuni -nib second_value -f
myTool -nqib --user Armakuni -f first_value second_value
myTool --user Armakuni -fnqib first_value second_value
```

###### Short names

Short named options (switch or valued options) consist of only one single character and must be preceded by a single dash. They may be grouped and have no strict order, so the following syntaxes are (almost) equivalent:

```bash
myTool -q -i -n
myTool -qin
myTool -i -nq
```

An argument requiring an additional value can, of course, only be in a group, if it's the last character, like this:
```console
myTool -q -i -n -v 5
myTool -qinv 5
```

###### Long names

Long names for arguments have to be prefixed with a double dash.

```
myTool --quiet
myTool --user MilkaCow
```

###### Order of options
The order of options (not required values) is generally not important. The only thing to consider is, that arguments are parsed in sequence, so if you define conflicting arguments, the last argument may be decisive. Consider:

```console
myTool --verbose --quiet
myTool --quiet --verbose
```

Here, if `quiet` and `verbose` define opposing values for the same key, the first version will go with the `quiet` value while the latter will result the `verbose` one.
The same applies for valued options:

```console
myTool --user MelittaMan -u GerdFroebe
myTool -u GerdFroebe --user MelittaMan
```

The respective last value ist taken for the configuration key for `user`.

### How to use

For command line options, you have to provide your definitions at first, like this:

```csharp
var definitions = new List<ArgumentDefinition>
{
	ArgumentDefinition.CreateSwitchOption('q', "quiet", "General:quiet", "true"),
	ArgumentDefinition.CreateSwitchOption('i', "General:interactive", "true"),
	ArgumentDefinition.CreateSwitchOption('d', "debug-mode", "debug_mode", "yes"),
	ArgumentDefinition.CreateValueOption('u', "user", "Account:user", false),
	ArgumentDefinition.CreateSwitchOption('n', "no-user", "Account:user", ""),
	ArgumentDefinition.CreateValueOption('m', "mode", "Second:mode", false),
	ArgumentDefinition.CreateRequiredValue("Second:verb"),
	ArgumentDefinition.CreateRequiredValue("Second:target"),
};
```

The you just create an ```ConfigurationBuilder``` and add your desired config sources. You will see different extension methods (like AddCommandLine()), depending on the namespace you declared to use in your code.

```csharp
using Microsoft.Extensions.Configuration;
using Interlook.Configuration.CommandLine;

/*
  snip
*/

var args = "-qid upload --mode simple www.remote.intranet.universe.org".Split(' ');

var root = new ConfigurationBuilder()
	.AddCommandLine(argString.Split(), definitions)
	.Build();

Assert.Equal("yes", root["debug_mode"]);
Assert.Equal("true", root["General:interactive"]);
Assert.Equal("simple", root["Second:mode"]);
Assert.Equal("upload", root["Second:verb"]);
Assert.Equal("www.remote.intranet.universe.org", root["Second:target"]);
```

Or consider the following Ini-File:

```ini
global_one=This is with whitespaces

#This shall be ignored

[Section1]

switch1
;switch2
redefine=before  
data1=alpha,bravo,tango
redefine=after  

#another=value

[Section2]

switch1=
data2="quoted with trailing spaces   "
switch1=yes
data3='   single-quoted with trailing and leading spaces  '
```

To load the configuration data from it, you would do something like this:

```csharp
using Interlook.Configuration.IniFile;
using Microsoft.Extensions.Configuration;

/*
  snip
*/

var root = new ConfigurationBuilder()
	.AddIniFile("IniFile1.cfg", optional: false, duplicateKeyBehavior: KeyNameBehavior.Update)
	.Build();

Assert.Equal("This is with whitespaces", root["global_one"]);

Assert.Equal(string.Empty, root["Section1:switch1"]);
Assert.Null(root["Section1:switch2"]);
Assert.Equal("after", root["Section1:redefine"]);
Assert.Equal("alpha,bravo,tango", root["Section1:data1"]);

Assert.Equal("yes", root["Section2:switch1"]);
Assert.Equal("quoted with trailing spaces   ", root["Section2:data2"]);
Assert.Equal("   single-quoted with trailing and leading spaces  ", root["Section2:data3"]);
```

Of course, you can combine several configuration sources:

```csharp
var builder = new ConfigurationBuilder()
	.AddIniFile("IniFile1.cfg", optional: false, duplicateKeyBehavior: KeyNameBehavior.Update)
	.AddCommandLine(argString.Split(), definitions)
	.AddJson(...)
...
```

because that's on of the crucial points of this configuration system. 

When combining sources, keep in mind that the execution/parsing order of the several sources is the same as they are declared. The first provider/source, that contains an assignment for a key, is generally the one which defines it. In the example above, the Ini-file could override all assignments of other sources, since it is processed first. Responsible for this behavior is `ConfigurationRoot`, so if you want to change this, that would probably be your starting point.

### TODO
- JSON provider (using Newtonsoft.Json and implementing handling of errors and duplicate keys of this library)
- maybe XML provider (as above)
- considering scripts, like python, shell etc, from where variables are imported afterwards; but that would go into a separate library