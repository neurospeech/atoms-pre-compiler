# Atoms Pre Compiler
Atoms.js precompiler service to generate HTML5 compliant HTML and JavaScript to run with CSP

AtomsPreCompiler compiles atoms markup into valid HTML5 by breaking up inline expressions into
generated JavaScript. This pre-compiled document runs in strict mode without using `eval` without compromising speed.

Usage
-----

Download nuget package Atoms.js PreCompiler

Modify web.config as shown below on IIS7 onwards

	 <system.webServer>
	 	<modules runAllManagedModulesForAllRequests="true">
	 		<add name="AtomPrecompilerModule" type="NeuroSpeech.AtomsPreCompiler.AtomsPreCompilerModule, NeuroSpeech.AtomsPreCompiler" />
	 	</modules>
	 </system.webServer>


Query String Parameters
-----------------------

Turn on/off pre compilation

    atoms-pre-compile=(yes/no) default=yes

Turn on/off debug mode

    atoms-pre-compile-mode=(debug/none) default=none