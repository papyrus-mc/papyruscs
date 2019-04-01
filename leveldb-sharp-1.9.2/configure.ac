AC_PREREQ([2.54])
AC_INIT([leveldb-sharp], [1.9.2], [https://github.com/meebey/leveldb-sharp/issues])
AC_CONFIG_MACRO_DIR([m4])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

AC_PROG_INSTALL

ASSEMBLY_DESCRIPTION="portable C# binding for LevelDB"
ASSEMBLY_VERSION="1.2"
AC_SUBST(ASSEMBLY_NAME, $PACKAGE)
AC_SUBST(ASSEMBLY_TITLE, $PACKAGE)
AC_SUBST(ASSEMBLY_DESCRIPTION)
AC_SUBST(ASSEMBLY_VERSION)

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_CHECK_MONO_MODULE(1.2.6)
SHAMROCK_FIND_MONO_RUNTIME
AC_SUBST(CLR, $MONO)
SHAMROCK_FIND_MONO_2_0_COMPILER
AC_SUBST(CSC, $MCS)
CSC_FLAGS="-debug -nowarn:1591"
AC_SUBST(CSC_FLAGS)

MCS_BASENAME=$(basename $MCS)
CLI_RUNTIME=
if test "$MCS_BASENAME" = "gmcs"; then
	CLI_RUNTIME=2.0
fi
if test "$MCS_BASENAME" = "dmcs"; then
	CLI_RUNTIME=4.0
fi

AC_PATH_PROG(XBUILD, xbuild, no)
if test "x$XBUILD" = "xno"; then
	AC_MSG_ERROR([You need to install xbuild])
fi
if test "$CLI_RUNTIME" = "2.0"; then
	XBUILD_FLAGS="/toolsversion:3.5 /p:TargetFrameworkVersion=v3.5"
fi
if test "$CLI_RUNTIME" = "4.0"; then
	XBUILD_FLAGS="/toolsversion:4.0 /p:TargetFrameworkVersion=v4.0"
fi
AC_SUBST(XBUILD_FLAGS)

AC_PATH_PROG(GACUTIL, gacutil)
if test x$GACUTIL = x; then
	AC_MSG_ERROR(You need to install gacutil)
fi
GACUTIL_FLAGS='-root $(DESTDIR)$(libdir)'
AC_SUBST(GACUTIL_FLAGS)

AC_ARG_ENABLE(pkg-config,
	AC_HELP_STRING([--disable-pkg-config],
		[Disable pkg-config install [default=NO]]),
	disable_pkg_config=yes, disable_pkg_config=no
)
AM_CONDITIONAL([INSTALL_PKG_CONFIG], test x$disable_pkg_config = xno)

AC_ARG_ENABLE(pkg-lib,
	AC_HELP_STRING([--disable-pkg-lib],
		[Disable library install [default=NO]]),
	disable_pkg_lib=yes, disable_pkg_lib=no
)
AM_CONDITIONAL([INSTALL_PKG_LIB], test x$disable_pkg_lib = xno)

AC_ARG_ENABLE(pkg-gac,
	AC_HELP_STRING([--disable-pkg-gac],
		[Disable GAC install [default=NO]]),
	disable_pkg_gac=yes, disable_pkg_gac=no
)
AM_CONDITIONAL([INSTALL_PKG_GAC], test x$disable_pkg_gac = xno)

AC_CONFIG_FILES([
	Makefile
	leveldb-sharp.pc
	AssemblyVersion.cs
])

AC_OUTPUT

if test "x$CLI_RUNTIME" = "x"; then
	CLI_RUNTIME=default
fi

AC_MSG_RESULT([
	Configuration summary for $PACKAGE_NAME ($VERSION)
        
	* Installation prefix: $prefix
	* Compiler:            $CSC
	* Target CLI runtime:  $CLI_RUNTIME
])
